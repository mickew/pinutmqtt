using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUTDotNetClient;
using Pinutmqtt.Extensions;
using Pinutmqtt.Models;

namespace Pinutmqtt.Services;

internal class NutUPSClientService : INutUPSClientService
{
    private readonly NUTClient _nUTClient;
    private readonly ILogger<NutUPSClientService> _logger;
    private readonly NutSettings _nutSettings;
    private ClientUPS? _clientUPS;
    private NutUpsStatus _upsStatus;

    public NutUPSClientService(ILogger<NutUPSClientService> logger, IOptions<NutSettings> nutSettings)
    {
        _logger = logger;
        _nutSettings = nutSettings.Value;
        _nUTClient = new NUTClient(_nutSettings.Host!, _nutSettings.Port);
        _upsStatus = NutUpsStatus.Unknown;
    }

    public async Task<UPSStaus> GetUPSStatusAsync(CancellationToken cancellationToken)
    {
        UPSStaus uPSStaus = new();
        if (!_nUTClient.IsConnected || _clientUPS == null)
        {
            _logger.LogWarning("UPS client is not connected.");
            _upsStatus = NutUpsStatus.CommunicationLost;
            throw new InvalidOperationException("UPS client is not connected.");
        }

        bool forceUpdate = true;
        var s = await _clientUPS!.GetVariableAsync("ups.status", forceUpdate);

        _upsStatus = NutUpsHelper.ParseStatusString(s.Value);

        uPSStaus.Status = _upsStatus.ToFriendlyString();

        double nominelWatts = double.NaN;
        if (double.IsNaN(_nutSettings.RealPowerNominelWatts))
        {
            s = await _clientUPS!.GetVariableAsync("ups.realpower.nominal");
            nominelWatts = double.Parse(s.Value);
        }
        else
        {
            nominelWatts = _nutSettings.RealPowerNominelWatts;
        }

        s = await _clientUPS!.GetVariableAsync("ups.load", forceUpdate);
        double loadPercent = double.Parse(s.Value, CultureInfo.InvariantCulture);
        uPSStaus.LoadPercent = (int)loadPercent;
        uPSStaus.LoadWatts = (int)(nominelWatts * loadPercent / 100.0);

        s = await _clientUPS!.GetVariableAsync("battery.runtime", forceUpdate);
        uPSStaus.Runtime = TimeSpan.FromSeconds(int.Parse(s.Value));

        s = await _clientUPS!.GetVariableAsync("battery.charge", forceUpdate);
        uPSStaus.BatteryPercent = int.Parse(s.Value, CultureInfo.InvariantCulture);

        s = await _clientUPS!.GetVariableAsync("battery.voltage", forceUpdate);
        uPSStaus.BatteryVoltage = double.Parse(s.Value, CultureInfo.InvariantCulture);

        s = await _clientUPS!.GetVariableAsync("input.voltage", forceUpdate);
        uPSStaus.InputVoltage = double.Parse(s.Value, CultureInfo.InvariantCulture);

        s = await _clientUPS!.GetVariableAsync("output.voltage", forceUpdate);
        uPSStaus.OutputVoltage = double.Parse(s.Value, CultureInfo.InvariantCulture);

        return uPSStaus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NutUPSClientService starting...");
        try
        {
            await _nUTClient.ConnectAsync();
        }
        catch (Exception)
        {
        }
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_nUTClient.IsConnected)
                    {
                        await _nUTClient.ConnectAsync();
                        List<ClientUPS> clientUPSes = await _nUTClient.GetUPSesAsync(true);
                        _clientUPS = clientUPSes.FirstOrDefault(u => u.Name == _nutSettings.UPSName);
                        _logger.LogInformation("NutUPSClientService started");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "The MQTT client  connection failed, retrying in 5 seconds...");
                }
                finally
                {
                    Task.Delay(5000, cancellationToken).Wait(cancellationToken);
                }
            }
        });
        if (_nUTClient.IsConnected)
        {
            List<ClientUPS> clientUPSes = await _nUTClient.GetUPSesAsync(true);
            _clientUPS = clientUPSes.FirstOrDefault(u => u.Name == _nutSettings.UPSName);
            _logger.LogInformation("NutUPSClientService started");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NutUPSClientService stopping....");
        if (_nUTClient.IsConnected)
        {
            await _nUTClient.DisconnectAsync();
        }
        _logger.LogInformation("NutUPSClientService stopped");
    }
}
