using System.Text.Json;
using JsonRpcX.Core.Context;
using JsonRpcX.Core.Methods;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

using IJsonRpcMessageHandler = JsonRpcMessageHandlerBase<byte[], byte[]?>;

internal class JsonRpcMessageHandler(
    IServiceProvider services,
    IJsonRpcMethodFactory factory,
    ILogger<JsonRpcMessageHandler> logger,
    IJsonRpcExceptionHandler? exceptionHandler = null
) : IJsonRpcMessageHandler(services, exceptionHandler)
{
    private readonly IJsonRpcMethodFactory _factory = factory;
    private readonly JsonSerializerOptions? _jsonOptions = services.GetService<JsonSerializerOptions>();
    private readonly ILogger _logger = logger;

    protected override async Task<IJsonRpcMessage?> HandleInternalAsync(
        byte[] message,
        IServiceScope scope,
        HttpContext httpCtx,
        CancellationToken ct = default
    )
    {
        if (message.Length == 0)
        {
            return null;
        }

        var ctxManager = scope.ServiceProvider.GetRequiredService<IJsonRpcContextManager>();
        try
        {
            // TODO: Uncommnet for JSON parse error
            // _ = JsonSerializer.Deserialize<int>(message, _jsonOptions);

            var request =
                JsonSerializer.Deserialize<JsonRpcRequest>(message, _jsonOptions)
                ?? throw new JsonException("JSON deserialization returned null");

            var ctx = new JsonRpcContext { Request = request, Http = httpCtx };
            ctxManager.SetContext(ctx);
            if (request.JsonRpc != JsonRpcConstants.Version)
            {
                throw new JsonRpcException(
                    ctx,
                    $"JSON-RPC version must be \"{JsonRpcConstants.Version}\" (received \"{request.JsonRpc}\")"
                );
            }

            var hasInvalidParams =
                request.Params.HasValue
                && !request.Params.Value.IsValueKindOneOf(JsonValueKind.Object, JsonValueKind.Array);

            if (hasInvalidParams)
            {
                throw new JsonRpcErrorException(
                    ctx,
                    (int)JsonRpcConstants.ErrorCode.ParseError,
                    $"Invalid \"params\" value kind: {request.Params?.ValueKind}"
                );
            }

            var invoker = GetMethodInvoker(scope, request.Method, ctx);
            var result = await invoker.InvokeAsync(request.Params, ct);

            return new JsonRpcResponseSuccess { Id = request.Id, Result = result };
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON error");
            throw new JsonRpcErrorException(null, (int)JsonRpcConstants.ErrorCode.ParseError, "JSON parse error");
        }
    }

    protected override byte[]? ConvertToOutputType(IJsonRpcMessage? response) =>
        response != null ? JsonSerializer.Serialize(response, _jsonOptions).GetUtf8Bytes() : null;

    private JsonRpcMethodInvoker GetMethodInvoker(IServiceScope scope, string method, JsonRpcContext ctx)
    {
        var @internal = _factory.CreateHandler(scope, method, ctx);
        return new JsonRpcMethodInvoker(@internal, ctx, _jsonOptions);
    }
}
