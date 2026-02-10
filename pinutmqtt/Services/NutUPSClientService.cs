using Microsoft.Extensions.Logging;

namespace Pinutmqtt.Services;

internal class NutUPSClientService : INutUPSClientService
{
    private readonly ILogger<NutUPSClientService> _logger;

    public NutUPSClientService(ILogger<NutUPSClientService> logger)
    {
        _logger = logger;
    }
}
