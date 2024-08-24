using JsonRpcX.Exceptions;

namespace JsonRpcX.Handlers;

internal abstract class MessageHandlerBase<TIn, TOut>(
    IServiceProvider services,
    IMessageExceptionHandler? exceptionHandler
) : IMessageHandler<TIn, TOut>
{
    private readonly IServiceProvider _services = services;
    private readonly IMessageExceptionHandler? _exceptionHandler = exceptionHandler;

    public async Task<TOut> HandleAsync(TIn message, HttpContext ctx, CancellationToken ct = default)
    {
        var scope = _services.CreateScope();
        try
        {
            var response = await HandleInternalAsync(message, scope, ctx, ct);
            ;
            return ConvertToOutputType(response);
        }
        catch (Exception ex)
        {
            if (_exceptionHandler != null)
            {
                var errorResponse = await _exceptionHandler.HandleAsync(ex, ct);
                if (errorResponse != null)
                {
                    // The exception was handled successfully
                    return ConvertToOutputType(errorResponse);
                }

                throw;
            }

            throw;
        }
        finally
        {
            scope.Dispose();
        }
    }

    /// <summary>
    /// Handles the incoming message in the given scope and context.
    /// </summary>
    protected abstract Task<object?> HandleInternalAsync(
        TIn message,
        IServiceScope scope,
        HttpContext ctx,
        CancellationToken ct = default
    );

    /// <summary>
    /// Converts/serializes the response to the desired output type.
    /// </summary>
    protected abstract TOut ConvertToOutputType(object? response);
}
