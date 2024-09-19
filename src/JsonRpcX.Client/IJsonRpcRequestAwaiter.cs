using JsonRpcX.Domain.Models;

namespace JsonRpcX.Client;

/// <summary>
/// Service for waiting for responses to asynchronous requests made using
/// bidirectional transports.
/// </summary>
public interface IJsonRpcRequestAwaiter
{
    /// <summary>
    /// Waits for a response to the given request ID.
    /// </summary>
    Task<JsonRpcResponse> WaitForResponseAsync(string clientId, string requestId, TimeSpan? timeout = null);

    /// <summary>
    /// Sets the response to the client.
    /// If a client is waiting for a response that matches the response ID,
    /// the wait task will be completed.
    /// </summary>
    void SetResponse(string clientId, JsonRpcResponse response);
}
