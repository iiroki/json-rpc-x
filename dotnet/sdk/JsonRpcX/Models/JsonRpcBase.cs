using System.Text.Json.Serialization;

namespace JsonRpcX.Models;

public abstract class JsonRpcBase : IJsonRpcMessage
{
    /// <summary>
    /// MUST be <c>"2.0"</c>.
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; private init; } = JsonRpcConstants.Version;

    [JsonPropertyName("id")]
    public string? Id { get; init; }
}
