using JsonRpcX.Exceptions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Exceptions;

/// <summary>
/// Default JSON RPC exception handler.<br />
/// <br />
/// This class can be inherited to further extend JSON RPC exception handling.
/// </summary>
internal class JsonRpcExceptionHandler : IJsonRpcExceptionHandler
{
    public Task<JsonRpcError?> HandleAsync(Exception ex, JsonRpcContext ctx, CancellationToken ct = default)
    {
        var error = ex is JsonRpcErrorException errorEx ? errorEx.Error : null;
        return Task.FromResult(error);
    }
}
