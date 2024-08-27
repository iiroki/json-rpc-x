using JsonRpcX.Core.Context;
using JsonRpcX.Core.Parsers;
using JsonRpcX.Exceptions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal abstract class JsonRpcMessageProcessorBase<TIn, TOut>(
    IServiceProvider services,
    IJsonRpcExceptionHandler? exceptionHandler = null
) : IJsonRpcMessageProcessor<TIn, TOut>
{
    private readonly IServiceProvider _services = services;

    // private readonly IJsonRpcMessageHandler<TIn> _handler = handler;
    private readonly IJsonRpcExceptionHandler? _exceptionHandler = exceptionHandler;

    public async Task<TOut> ProcessAsync(TIn message, JsonRpcContext ctx, CancellationToken ct = default)
    {
        // 1. Initialize the request scope
        var scope = _services.CreateScope();
        try
        {
            // 2. Get the handler
            var handler = scope.ServiceProvider.GetRequiredService<IJsonRpcMessageHandler<TIn>>();

            // 3. Set the request context to the scope
            var ctxManager = scope.ServiceProvider.GetRequiredService<IJsonRpcContextManager>();
            ctxManager.SetContext(ctx);

            // 4. Parse the request
            var parser =
                scope.ServiceProvider.GetService<IJsonRpcRequestParser<TIn>>()
                ?? throw new JsonRpcParseException($"No parser found for type: {typeof(TIn).Name}");

            var request = parser.Parse(message) ?? throw new JsonRpcParseException("Received null");

            // Update the context with the request
            ctx = ctx.WithRequest(request);
            ctxManager.SetContext(ctx);

            // 3. Handle the request
            var response = await handler.HandleAsync(request, ctx, ct);

            // 4. Convert to the desired output type
            return ConvertToOutputType(response);
        }
        catch (Exception ex)
        {
            JsonRpcResponseError? errorResponse = null;
            if (_exceptionHandler != null)
            {
                var error = await _exceptionHandler.HandleAsync(ex, ctx, ct);
                if (error != null)
                {
                    // The exception was handled successfully
                    errorResponse = new JsonRpcResponseError { Id = ctx.Request?.Id, Error = error };
                }
            }

            // Fallback to the default internal error
            errorResponse ??= new JsonRpcResponseError
            {
                Id = ctx.Request?.Id,
                Error = new JsonRpcError
                {
                    Code = (int)JsonRpcConstants.ErrorCode.InternalError,
                    Message = "Unknown error",
                    Data = new { DetailedMessage = ex.Message },
                },
            };

            return ConvertToOutputType(errorResponse.ToResponse());
        }
        finally
        {
            scope.Dispose();
        }
    }

    /// <summary>
    /// Converts/serializes the response to the desired output type.
    /// </summary>
    protected abstract TOut ConvertToOutputType(JsonRpcResponse? response);
}
