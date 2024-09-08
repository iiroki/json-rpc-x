using System.Text.Json.Serialization;
using JsonRpcX.Domain.Constants;

namespace JsonRpcX.Domain.Models;

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
