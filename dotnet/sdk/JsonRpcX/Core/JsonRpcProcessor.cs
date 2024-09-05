using JsonRpcX.Core.Context;
using JsonRpcX.Core.Requests;
using JsonRpcX.Core.Serialization;
using JsonRpcX.Exceptions;
using JsonRpcX.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Core;

internal class JsonRpcProcessor<TIn, TOut>(
    IServiceScopeFactory scopeFactory,
    IJsonRpcRequestSerializer<TIn> requestSerializer,
    IJsonRpcResponseSerializer<TOut> responseSerializer,
    ILogger<JsonRpcProcessor<TIn, TOut>> logger
) : IJsonRpcProcessor<TIn, TOut>
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IJsonRpcRequestSerializer<TIn> _requestSerializer = requestSerializer;
    private readonly IJsonRpcResponseSerializer<TOut> _responseSerializer = responseSerializer;
    private readonly ILogger _logger = logger;

    public async Task<TOut?> ProcessAsync(TIn message, JsonRpcContext ctx, CancellationToken ct = default)
    {
        var scope = _scopeFactory.CreateScope();

        JsonRpcResponse? response;
        try
        {
            // 1. Initialize the context
            var ctxManager = scope.ServiceProvider.GetRequiredService<IJsonRpcContextManager>();
            ctxManager.SetContext(ctx);

            // 2. Update the context with the request
            var request = _requestSerializer.Parse(message) ?? throw new JsonRpcParseException("Received null");
            ctx = ctx.WithRequest(request);
            ctxManager.SetContext(ctx);

            // 3. Handle the request
            var handler = scope.ServiceProvider.GetRequiredService<IJsonRpcRequestHandler>();
            response = await handler.HandleAsync(request, ctx, ct);

            // 4. Serialize the response
            return _responseSerializer.Serialize(response);
        }
        catch (Exception ex)
        {
            JsonRpcError? error = null;

            // 1. If an exception handler is defined, try to handle the error with it.
            var exceptionHandler = scope.ServiceProvider.GetService<IJsonRpcExceptionHandler>();
            if (exceptionHandler != null)
            {
                error = await exceptionHandler.HandleAsync(ex, ct);
            }

            // 2. If the error is still not defined, create a default error response.
            if (error == null)
            {
                if (ex is JsonRpcErrorException errorEx)
                {
                    error = errorEx.Error;
                }
                else
                {
                    _logger.LogError(ex, "Unknown error");
                    error = CreateUnknownError(ex);
                }
            }

            response = new JsonRpcResponseError { Id = ctx.Request?.Id, Error = error }.ToResponse();
            return _responseSerializer.Serialize(response);
        }
        finally
        {
            scope.Dispose();
        }
    }

    private static JsonRpcError CreateUnknownError(Exception ex) =>
        new()
        {
            Code = (int)JsonRpcConstants.ErrorCode.InternalError,
            Message = "Unknown error",
            Data = new { ex.Message },
        };
}
