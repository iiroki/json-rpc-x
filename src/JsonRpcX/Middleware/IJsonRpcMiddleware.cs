namespace JsonRpcX.Middleware;

/// <summary>
/// JSON RPC middleware.
/// </summary>
public interface IJsonRpcMiddleware
{
    Task HandleAsync(CancellationToken ct = default);
}
