using Microsoft.Extensions.Logging;
using Pinutmqtt.Models;
using Pinutmqtt.Services;
using Quartz;

namespace Pinutmqtt.Jobs;

internal class GetUPSStatusJob : IJob
{
    private readonly ILogger<GetUPSStatusJob> _logger;
    private readonly INutUPSClientService _nutUPSClientService;
    private readonly IMqttClientService _mqttClientService;
    private static UPSStaus _lastUPSStatus = new();

    public GetUPSStatusJob(ILogger<GetUPSStatusJob> logger, INutUPSClientService nutUPSClientService, IMqttClientService mqttClientService)
    {
        _logger = logger;
        _nutUPSClientService = nutUPSClientService;
        _mqttClientService = mqttClientService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Executing GetUPSStatusJob at {Time}", DateTimeOffset.Now);
        var (lastReadingOk, uPSStatus) = await GetUPSStatusAsync(context.CancellationToken);
        if (lastReadingOk)
        {
            if (await _mqttClientService.IsConnectedAsync)
            {
                await _mqttClientService.PublishStatusAsync(uPSStatus);
            }
            else
            {
                _logger.LogWarning("MQTT client is not connected. Skipping publish.");

            }
        }
    }

    private async Task<(bool lastReadingOk, UPSStaus uPSStatus)> GetUPSStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            UPSStaus status = await _nutUPSClientService.GetUPSStatusAsync(cancellationToken);
            _lastUPSStatus = status;
            return (true, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ups status");
        }
        return (false, _lastUPSStatus);
    }
}
