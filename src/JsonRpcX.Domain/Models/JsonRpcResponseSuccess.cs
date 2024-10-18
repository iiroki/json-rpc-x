using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonRpcX.Domain.Models;

// /// <summary>
// /// JSON-RPC 2.0 Response (success).
// /// </summary>
// public class JsonRpcResponseSuccess : JsonRpcBase
// {
//     [JsonPropertyName("result")]
//     [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
//     public JsonElement? Result { get; init; }

//     public JsonRpcResponse ToResponse() => new(this);
// }
