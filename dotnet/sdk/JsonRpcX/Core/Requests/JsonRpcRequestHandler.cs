using System.Text.Json;
using JsonRpcX.Constants;
using JsonRpcX.Core.Methods;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Middleware;
using JsonRpcX.Models;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Core.Requests;

internal class JsonRpcRequestHandler(
    IJsonRpcMethodFactory factory,
    IEnumerable<IJsonRpcMiddleware> middleware,
    ILogger<JsonRpcRequestHandler> logger
) : IJsonRpcRequestHandler
{
    private readonly IJsonRpcMethodFactory _factory = factory;
    private readonly List<IJsonRpcMiddleware> _middleware = middleware.ToList();
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
                    (int)JsonRpcErrorCode.ParseError,
                    $"Invalid \"params\" value kind: {request.Params?.ValueKind}"
                );
            }

            var invoker = _factory.Create(request.Method);

            // Process middleware after acquiring the invoker, but before invoking the method.
            foreach (var m in _middleware)
            {
                await m.HandleAsync(ct);
            }

            var result = await invoker.InvokeAsync(request.Params, ct);
            return new JsonRpcResponseSuccess { Id = request.Id, Result = result }.ToResponse();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON error");
            throw new JsonRpcErrorException((int)JsonRpcErrorCode.ParseError, "JSON parse error");
        }
    }
}
