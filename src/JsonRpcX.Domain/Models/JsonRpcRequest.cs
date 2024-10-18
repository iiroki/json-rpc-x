using System.Text.Json;
using System.Text.Json.Serialization;
using JsonRpcX.Domain.Constants;

namespace JsonRpcX.Domain.Models;

/// <summary>
/// JSON-RPC 2.0 Request.
/// </summary>
public class JsonRpcRequest : JsonRpcBase
{
    public JsonRpcRequest()
    {
        JsonRpc = JsonRpcConstants.Version;
    }

    [JsonConstructor]
    public JsonRpcRequest(string jsonRpc)
    {
        JsonRpc = jsonRpc;
    }

    [JsonPropertyName("method")]
    [JsonPropertyOrder(3)]
    public required string Method { get; init; }

    /// <summary>
    /// MUST be an object or an array.
    /// </summary>
    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(4)]
    public JsonElement? Params { get; init; }

    [JsonIgnore]
    public bool IsNotification => Id == null;
}
