using System.Text.Json.Serialization;

namespace Pinutmqtt.Models;

internal record AnnouncePayload(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("mac")] string Mac,
    [property: JsonPropertyName("ip")] string Ip
 );
