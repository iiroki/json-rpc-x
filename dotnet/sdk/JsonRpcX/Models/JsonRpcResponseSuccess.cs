using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonRpcX.Models;

/// <summary>
/// JSON-RPC 2.0 Response (success).
/// </summary>
public class JsonRpcResponseSuccess : JsonRpcBase
{
    [JsonPropertyName("result")]
    public object? Result { get; init; }
}
