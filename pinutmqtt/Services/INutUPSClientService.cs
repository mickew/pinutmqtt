using Microsoft.Extensions.Hosting;
using Pinutmqtt.Models;

namespace Pinutmqtt.Services;

internal interface INutUPSClientService : IHostedService
{
    Task<UPSStaus> GetUPSStatusAsync(CancellationToken cancellationToken);
}
