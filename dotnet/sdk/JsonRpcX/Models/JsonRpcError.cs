using System.Text.Json.Serialization;

namespace JsonRpcX.Models;

/// <summary>
/// JSON-RPC 2.0 Error.
/// </summary>
public class JsonRpcError
{
    [JsonPropertyName("code")]
    public required int Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("data")]
    public object? Data { get; init; }
}
