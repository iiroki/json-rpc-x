using JsonRpcX.Domain.Models;

namespace JsonRpcX.Client;

public interface IJsonRpcRequestAwaiter
{
    Task<JsonRpcResponse> WaitForResponseAsync(string clientId, string requestId, TimeSpan? timeout = null);

    void SetResponse(string clientId, JsonRpcResponse response);
}
