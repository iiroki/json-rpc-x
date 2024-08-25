using System.Text.Json.Serialization;

namespace JsonRpcX.Models;

/// <summary>
/// JSON-RPC 2.0 Response (error).
/// </summary>
public class JsonRpcResponseError : JsonRpcBase
{
    [JsonPropertyName("error")]
    public required JsonRpcError Error { get; init; }

    public JsonRpcResponse ToResponse() => new(this);
}
