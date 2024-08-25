using System.Text.Json;
using JsonRpcX.Core.Methods;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal class JsonRpcMessageHandler(
    IJsonRpcMethodFactory factory,
    ILogger<JsonRpcMessageHandler> logger,
    JsonSerializerOptions jsonOptions
) : IJsonRpcMessageHandler<byte[]>, IJsonRpcMessageHandler<string>
{
    private readonly IJsonRpcMethodFactory _factory = factory;
    private readonly JsonSerializerOptions? _jsonOptions = jsonOptions;
    private readonly ILogger _logger = logger;

    public JsonRpcRequest? Parse(byte[] message) => JsonSerializer.Deserialize<JsonRpcRequest>(message, _jsonOptions);

    public JsonRpcRequest? Parse(string message) => Parse(message.GetUtf8Bytes());

    public async Task<JsonRpcResponse?> HandleAsync(
        JsonRpcRequest request,
        JsonRpcContext ctx,
        CancellationToken ct = default
    )
    {
        try
        {
            if (request.JsonRpc != JsonRpcConstants.Version)
            {
                throw new JsonRpcException(
                    $"JSON-RPC version must be \"{JsonRpcConstants.Version}\" (received \"{request.JsonRpc}\")"
                );
            }

            var hasInvalidParams =
                request.Params.HasValue
                && !request.Params.Value.IsValueKindOneOf(JsonValueKind.Object, JsonValueKind.Array);

            if (hasInvalidParams)
            {
                throw new JsonRpcErrorException(
                    (int)JsonRpcConstants.ErrorCode.ParseError,
                    $"Invalid \"params\" value kind: {request.Params?.ValueKind}"
                );
            }

            var invoker = GetMethodInvoker(request.Method);
            var result = await invoker.InvokeAsync(request.Params, ct);
            return new JsonRpcResponseSuccess { Id = request.Id, Result = result }.ToResponse();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON error");
            throw new JsonRpcErrorException((int)JsonRpcConstants.ErrorCode.ParseError, "JSON parse error");
        }
    }

    private JsonRpcMethodInvoker GetMethodInvoker(string method)
    {
        var invocation = _factory.CreateInvocation(method);
        return new JsonRpcMethodInvoker(invocation, _jsonOptions);
    }
}
