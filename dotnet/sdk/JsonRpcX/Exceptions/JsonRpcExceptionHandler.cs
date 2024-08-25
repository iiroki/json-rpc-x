using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

/// <summary>
/// Default JSON RPC exception handler.<br />
/// <br />
/// This class can be inherited to further extend JSON RPC exception handling.
/// </summary>
public class JsonRpcExceptionHandler : IJsonRpcExceptionHandler
{
    public Task<JsonRpcError?> HandleAsync(Exception ex, JsonRpcContext ctx, CancellationToken ct = default)
    {
        if (ex is JsonRpcErrorException errorEx)
        {
            // TODO
        }
        else if (ex is JsonRpcAuthException authEx)
        {
            // TODO
        }

        return Task.FromResult<JsonRpcError?>(null);
    }
}
