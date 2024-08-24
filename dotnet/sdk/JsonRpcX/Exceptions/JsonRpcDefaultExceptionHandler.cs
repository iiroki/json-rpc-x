using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

/// <summary>
/// Default JSON RPC exception handler.<br />
/// <br />
/// This class can be inherited to further extend JSON RPC exception handling.
/// </summary>
public class JsonRpcDefaultExceptionHandler : IJsonRpcExceptionHandler
{
    public Task<JsonRpcError?> HandleAsync(Exception ex, CancellationToken ct = default)
    {
        if (ex.GetType() == typeof(JsonRpcErrorException))
        {
            // TODO
        }

        return Task.FromResult<JsonRpcError?>(null);
    }
}
