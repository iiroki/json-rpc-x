using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonRpcX.Domain.Models;

/// <summary>
/// JSON-RPC 2.0 Request.
/// </summary>
public class JsonRpcRequest : JsonRpcBase
{
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    /// <summary>
    /// MUST be an object or an array.
    /// </summary>
    [JsonPropertyName("params")]
    public JsonElement? Params { get; init; }

    [JsonIgnore]
    public bool IsNotification => Id == null;
}
