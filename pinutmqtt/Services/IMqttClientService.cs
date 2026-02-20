using Microsoft.Extensions.Hosting;
using Pinutmqtt.Models;

namespace Pinutmqtt.Services;

internal interface IMqttClientService : IHostedService
{
    Task<bool> IsConnectedAsync { get; }

    Task<bool> PublishStatusAsync(UPSStaus uPSStaus);
}
