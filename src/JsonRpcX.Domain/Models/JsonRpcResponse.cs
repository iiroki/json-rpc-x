using System.Text.Json;
using System.Text.Json.Serialization;
using JsonRpcX.Domain.Constants;

namespace JsonRpcX.Domain.Models;

public class JsonRpcResponse : JsonRpcBase
{
    public JsonRpcResponse()
    {
        JsonRpc = JsonRpcConstants.Version;
    }

    [JsonConstructor]
    public JsonRpcResponse(string jsonRpc)
    {
        JsonRpc = jsonRpc;
    }

    [JsonPropertyName("result")]
    [JsonPropertyOrder(3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // Default = Undefined
    public JsonElement Result { get; init; }

    [JsonPropertyName("error")]
    [JsonPropertyOrder(3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? Error { get; init; }

    [JsonIgnore]
    public bool IsSuccess => Error == null;
}
