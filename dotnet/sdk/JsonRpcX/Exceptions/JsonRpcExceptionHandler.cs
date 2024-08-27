using JsonRpcX.Models;
using JsonRpcX.Options;

namespace JsonRpcX.Exceptions;

/// <summary>
/// Default JSON RPC exception handler.<br />
/// <br />
/// This class can be inherited to further extend JSON RPC exception handling.
/// </summary>
public class JsonRpcExceptionHandler(JsonRpcExceptionOptions opt) : IJsonRpcExceptionHandler
{
    private readonly JsonRpcExceptionOptions _opt = opt;

    public Task<JsonRpcError?> HandleAsync(Exception ex, JsonRpcContext ctx, CancellationToken ct = default)
    {
        JsonRpcError? error = null;
        if (ex is JsonRpcErrorException errorEx)
        {
            error = errorEx.Error;
        }
        else if (ex is JsonRpcAuthException authEx)
        {
            var hasUser = ctx.Http.User.Identity?.IsAuthenticated ?? false;
            error = new JsonRpcError
            {
                Code = hasUser ? _opt.AuthorizationErrorCode : _opt.AuthenticationErrorCode,
                Message = hasUser ? "Authorization error" : "Authentication error",
                Data = new { Detail = authEx.Message },
            };
        }

        return Task.FromResult(error);
    }
}
