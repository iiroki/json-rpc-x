using System.Text.Json.Serialization;
using JsonRpcX.Constants;

namespace JsonRpcX.Models;

public abstract class JsonRpcBase
{
    /// <summary>
    /// MUST be <c>"2.0"</c>.
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; private init; } = JsonRpcConstants.Version;

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }
}
