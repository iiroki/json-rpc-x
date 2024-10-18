using System.Text.Json.Serialization;

namespace JsonRpcX.Domain.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public abstract class JsonRpcBase
{
    /// <summary>
    /// MUST be <c>"2.0"</c>.
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    [JsonPropertyOrder(1)]
    public string? JsonRpc { get; init; }

    [JsonPropertyName("id")]
    [JsonPropertyOrder(2)]
    public string? Id { get; init; }
}
