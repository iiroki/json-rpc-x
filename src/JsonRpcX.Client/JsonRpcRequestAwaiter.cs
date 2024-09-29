using System.Collections.Concurrent;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Client;

internal class JsonRpcRequestAwaiter : IJsonRpcRequestAwaiter
{
    private readonly ConcurrentDictionary<RequestKey, TaskCompletionSource<JsonRpcResponse>> _requests = [];

    /// <summary>
    /// Count of request currently being awaited.
    /// </summary>
    public int Count => _requests.Count;

    public async Task<JsonRpcResponse> WaitForResponseAsync(string clientId, string requestId, TimeSpan? timeout = null)
    {
        var taskSource = new TaskCompletionSource<JsonRpcResponse>();

        // Cancel the task after the specified timeout
        timeout ??= TimeSpan.FromSeconds(5);
        var ct = new CancellationTokenSource(timeout.Value).Token;
        ct.Register(() => taskSource.SetCanceled(ct));

        var key = new RequestKey(clientId, requestId);
        _requests.TryAdd(key, taskSource);
        try
        {
            return await taskSource.Task;
        }
        catch (OperationCanceledException ex)
        {
            throw new JsonRpcTimeoutException("JSON RPC timeout exceeded", timeout.Value, ex);
        }
        finally
        {
            _requests.TryRemove(key, out _);
        }
    }

    public void SetResponse(string clientId, JsonRpcResponse response)
    {
        if (string.IsNullOrEmpty(response.Id))
        {
            return;
        }

        var key = new RequestKey(clientId, response.Id);
        if (_requests.TryGetValue(key, out var taskSource))
        {
            taskSource.SetResult(response);
        }
    }

    private record RequestKey(string ClientId, string RequestId);
}
