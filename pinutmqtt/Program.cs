
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pinutmqtt.Services;
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
