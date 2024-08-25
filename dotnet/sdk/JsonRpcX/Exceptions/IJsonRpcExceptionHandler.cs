using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

/// <summary>
/// Handler for exceptions thrown during JSON RPC request processing.
/// </summary>
public interface IJsonRpcExceptionHandler
{
    Task<JsonRpcError?> HandleAsync(Exception ex, JsonRpcContext ctx, CancellationToken ct = default);
}
