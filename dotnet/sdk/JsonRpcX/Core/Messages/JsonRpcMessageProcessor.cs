using JsonRpcX.Core.Context;
using JsonRpcX.Core.Requests;
using JsonRpcX.Core.Serialization;
using JsonRpcX.Exceptions;
using JsonRpcX.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Core.Messages;

internal class JsonRpcMessageProcessor<TIn, TOut>(
    IServiceScopeFactory scopeFactory,
    IJsonRpcRequestSerializer<TIn> requestSerializer,
    IJsonRpcResponseSerializer<TOut> responseSerializer,
    IJsonRpcExceptionHandler? exceptionHandler = null
) : IJsonRpcMessageProcessor<TIn, TOut>
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IJsonRpcRequestSerializer<TIn> _requestSerializer = requestSerializer;
    private readonly IJsonRpcResponseSerializer<TOut> _responseSerializer = responseSerializer;
    private readonly IJsonRpcExceptionHandler? _exceptionHandler = exceptionHandler;

    public async Task<TOut?> ProcessAsync(TIn message, JsonRpcContext ctx, CancellationToken ct = default)
    {
        // Initialize the request scope
        var scope = _scopeFactory.CreateScope();
        try
        {
            // Set the request context to the scope
            var ctxManager = scope.ServiceProvider.GetRequiredService<IJsonRpcContextManager>();
            ctxManager.SetContext(ctx);

            // Get the handler
            var handler = scope.ServiceProvider.GetRequiredService<IJsonRpcRequestHandler>();

            // Parse the request
            var request = _requestSerializer.Parse(message) ?? throw new JsonRpcParseException("Received null");

            // Update the context with the request
            ctx = ctx.WithRequest(request);
            ctxManager.SetContext(ctx);

            // Handle the request
            var response = await handler.HandleAsync(request, ctx, ct);

            // Serialize the result
            return _responseSerializer.Serialize(response);
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
                    Data = new { ex.Message },
                },
            };

            return _responseSerializer.Serialize(errorResponse.ToResponse());
        }
        finally
        {
            scope.Dispose();
        }
    }
}
