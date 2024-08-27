using System.Text.Json;
using JsonRpcX.Core.Methods;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Models;

namespace JsonRpcX.Core.Requests;

internal class JsonRpcRequestHandler(IJsonRpcMethodFactory factory, ILogger<JsonRpcRequestHandler> logger)
    : IJsonRpcRequestHandler
{
    private readonly IJsonRpcMethodFactory _factory = factory;
    private readonly ILogger _logger = logger;

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

            var invoker = _factory.Create(request.Method);
            var result = await invoker.InvokeAsync(request.Params, ct);
            return new JsonRpcResponseSuccess { Id = request.Id, Result = result }.ToResponse();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON error");
            throw new JsonRpcErrorException((int)JsonRpcConstants.ErrorCode.ParseError, "JSON parse error");
        }
    }
}
