using JsonRpcX.Client;
using JsonRpcX.Core.Context;
using JsonRpcX.Core.Requests;
using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Core;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;
using JsonRpcX.Transport.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Core;

internal class JsonRpcProcessor<TIn, TOut>(
    IServiceScopeFactory scopeFactory,
    IJsonRpcMessageSerializer<TIn> messageSerializer,
    IJsonRpcRequestAwaiter requestAwaiter,
    IJsonRpcRequestSerializer<TIn> requestSerializer,
    IJsonRpcResponseSerializer<TOut> responseSerializer,
    ILogger<JsonRpcProcessor<TIn, TOut>> logger
) : IJsonRpcProcessor<TIn, TOut>
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IJsonRpcMessageSerializer<TIn> _messageSerializer = messageSerializer;
    private readonly IJsonRpcRequestAwaiter _requestAwaiter = requestAwaiter;

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

            // TODO: Do not return response parse errors!
            // 2. Check if the received message is a request or a response:
            // - Response -> Set the response to the request awaiter
            // - Request -> Process the request with the normal JSON RPC pipeline.
            var (req, res) = _messageSerializer.Parse(message);
            if (res != null && !string.IsNullOrEmpty(ctx.ClientId))
            {
                _requestAwaiter.SetResponse(ctx.ClientId, res);
                return default;
            }

            if (req == null)
            {
                return default; // Should not happen, but let's just ignore these messages!
            }

            // 3. Update the context with the request
            ctx = ctx.WithRequest(req);
            ctxManager.SetContext(ctx);

            // 4. Handle the request
            var handler = scope.ServiceProvider.GetRequiredService<IJsonRpcRequestHandler>();
            response = await handler.HandleAsync(req, ctx, ct);

            // 5. Serialize the response
            return req.IsNotification ? default : _responseSerializer.Serialize(response);
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
            Code = (int)JsonRpcErrorCode.InternalError,
            Message = "Unknown error",
            Data = new { ex.Message },
        };
}
