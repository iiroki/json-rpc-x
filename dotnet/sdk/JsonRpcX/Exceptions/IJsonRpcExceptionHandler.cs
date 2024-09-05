using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

/// <summary>
/// Handler for exceptions thrown during JSON RPC request processing.
/// </summary>
public interface IJsonRpcExceptionHandler
{
    /// <summary>
    /// Handles the exception by creating a JSON RPC error from the exception.<br />
    /// <br />
    /// If the exception is not handled and a JSON RPC error is not created,
    /// the error handling will use the default error handling as the fallback.
    /// </summary>
    Task<JsonRpcError?> HandleAsync(Exception ex, CancellationToken ct = default);
}
