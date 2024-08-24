using JsonRpcX.Exceptions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal abstract class JsonRpcMessageHandlerBase<TIn, TOut>(
    IServiceProvider services,
    IJsonRpcExceptionHandler? exceptionHandler
) : IJsonRpcMessageHandler<TIn, TOut>
{
    private readonly IServiceProvider _services = services;
    private readonly IJsonRpcExceptionHandler? _exceptionHandler = exceptionHandler;

    public async Task<TOut> HandleAsync(TIn message, HttpContext ctx, CancellationToken ct = default)
    {
        var scope = _services.CreateScope();
        try
        {
            var response = await HandleInternalAsync(message, scope, ctx, ct);
            return ConvertToOutputType(response);
        }
        catch (Exception ex)
        {
            IJsonRpcMessage? response = null;
            if (_exceptionHandler != null)
            {
                var error = await _exceptionHandler.HandleAsync(ex, ct);
                if (error != null)
                {
                    // The exception was handled successfully
                    response = new JsonRpcResponseError { Error = error };
                }
            }

            // Fallback to the default internal error
            response ??= new JsonRpcResponseError
            {
                Error = new JsonRpcError
                {
                    Code = (int)JsonRpcConstants.ErrorCode.InternalError,
                    Message = "Unknown error",
                    Data = new { DetailedMessage = ex.Message },
                },
            };

            return ConvertToOutputType(response);
        }
        finally
        {
            scope.Dispose();
        }
    }

    /// <summary>
    /// Handles the incoming message in the given scope and context.
    /// </summary>
    protected abstract Task<IJsonRpcMessage?> HandleInternalAsync(
        TIn message,
        IServiceScope scope,
        HttpContext ctx,
        CancellationToken ct = default
    );

    /// <summary>
    /// Converts/serializes the response to the desired output type.
    /// </summary>
    protected abstract TOut ConvertToOutputType(IJsonRpcMessage? response);
}
