
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pinutmqtt.Jobs;
using Pinutmqtt.Models;
using Pinutmqtt.Services;
using Quartz;
using Serilog;

const string VersionArgs = "--version";
const string SerilogOutputTemplate = "[{Timestamp:HH:mm:ss} {SourceContext} [{Level}] {Message}{NewLine}{Exception}";

var printVersion = args.Any(x => x == VersionArgs);
if (printVersion)
{
    Console.WriteLine(GetVersion());
    return;
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(GetConfiguration(args))
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: SerilogOutputTemplate)
    .CreateBootstrapLogger();

HostApplicationBuilderSettings settings = new()
{
    Args = args,
    Configuration = new ConfigurationManager(),
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production",
};

HostApplicationBuilder builder = Host.CreateApplicationBuilder(settings);

Log.ForContext<Program>().Information("Starting Pinutmqtt {Version}", GetVersion());

builder.Services.AddOptions<NutSettings>()
    .Bind(builder.Configuration.GetSection(NutSettings.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<MqttSettings>()
    .BindConfiguration(MqttSettings.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: SerilogOutputTemplate)
;
});

builder.Services.AddSingleton<INutUPSClientService, NutUPSClientService>();
builder.Services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetService<INutUPSClientService>()!);
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetService<IMqttClientService>()!);

var everySeconds = builder.Configuration.GetValue<int?>("UPSReadIntervalSeconds") ?? 30;


builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey(nameof(GetUPSStatusJob));
    options
        .AddJob<GetUPSStatusJob>(jobKey, (IJobConfigurator job) => { }) // Explicitly specify the delegate type
        .AddTrigger(trigger =>
            trigger
                .ForJob(jobKey)
                .WithSimpleSchedule(schedule =>
                    schedule.WithInterval(TimeSpan.FromSeconds(everySeconds)).RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});



IHost host = builder.Build();
await host.RunAsync();

static string GetVersion()
{
    Assembly currentAssembly = typeof(Program).Assembly;
    if (currentAssembly == null)
    {
        currentAssembly = Assembly.GetCallingAssembly();
    }
    var version = $"{currentAssembly.GetName().Version!.Major}.{currentAssembly.GetName().Version!.Minor}.{currentAssembly.GetName().Version!.Build}";
    return version ?? "?.?.?";
}

static IConfiguration GetConfiguration(string[] args)
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();
    return configuration;
}
