using JsonRpcX.Client;
using JsonRpcX.Context;
using JsonRpcX.Domain;
using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;
using JsonRpcX.Requests;
using JsonRpcX.Transport.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonRpcX;

internal class JsonRpcProcessor<TIn, TOut>(
    IServiceScopeFactory scopeFactory,
    IJsonRpcInSerializer<TIn> parser,
    IJsonRpcRequestAwaiter requestAwaiter,
    IJsonRpcOutSerializer<TOut> serializer,
    ILogger<JsonRpcProcessor<TIn, TOut>> logger
) : IJsonRpcProcessor<TIn, TOut>
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IJsonRpcInSerializer<TIn> _parser = parser;
    private readonly IJsonRpcRequestAwaiter _requestAwaiter = requestAwaiter;
    private readonly IJsonRpcOutSerializer<TOut> _serializer = serializer;
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

            // 2. Check if the received message is a request or a response:
            //     - Response -> Set the response to the request awaiter
            //     - Request -> Process the request with the normal JSON RPC pipeline.
            var (req, res) = _parser.Parse(message);
            if (res != null && !string.IsNullOrEmpty(ctx.ClientId))
            {
                _requestAwaiter.SetResponse(ctx.ClientId, res);
                return default;
            }

            if (req == null)
            {
                // This can happen when a non-critical parsing fails
                return default;
            }

            // 3. Update the context with the request
            ctx = ctx.WithRequest(req);
            ctxManager.SetContext(ctx);

            // 4. Handle the request
            var handler = scope.ServiceProvider.GetRequiredService<IJsonRpcRequestHandler>();
            response = await handler.HandleAsync(req, ctx, ct);

            // 5. Serialize the response
            return req.IsNotification ? default : _serializer.Serialize(response);
        }
        catch (Exception ex)
        {
            // 0. Do not send responses for notification or cancelled requests
            if (ct.IsCancellationRequested || (ctx.Request != null && ctx.Request.IsNotification))
            {
                return default;
            }

            JsonRpcError? error = null;

            // 1. If an exception handler is defined, try to handle the error with it.
            var exceptionHandler = scope.ServiceProvider.GetService<IJsonRpcExceptionHandler>();
            if (exceptionHandler != null)
            {
                error = await exceptionHandler.HandleAsync(ctx, ex, ct);
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

            response = new JsonRpcResponse { Id = ctx.Request?.Id, Error = error };
            return _serializer.Serialize(response);
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
