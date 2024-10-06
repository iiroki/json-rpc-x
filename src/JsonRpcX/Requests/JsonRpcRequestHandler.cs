using System.Text.Json;
using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Domain.Models;
using JsonRpcX.Extensions;
using JsonRpcX.Methods;
using JsonRpcX.Middleware;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Requests;

internal class JsonRpcRequestHandler(
    IJsonRpcMethodFactory factory,
    IEnumerable<IJsonRpcMiddleware> middleware,
    ILogger<JsonRpcRequestHandler> logger,
    JsonSerializerOptions? jsonOpt = null
) : IJsonRpcRequestHandler
{
    private readonly IJsonRpcMethodFactory _factory = factory;
    private readonly List<IJsonRpcMiddleware> _middleware = middleware.ToList();
    private readonly ILogger _logger = logger;
    private readonly JsonSerializerOptions? _jsonOpt = jsonOpt;

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

            var invoker = _factory.Create(request.Method);

            var hasInvalidParams =
                request.Params.HasValue
                && !request.Params.Value.IsValueKindOneOf(JsonValueKind.Object, JsonValueKind.Array);

            if (hasInvalidParams)
            {
                throw new JsonRpcInvalidParamsException($"Invalid \"params\" value kind: {request.Params?.ValueKind}");
            }

            // Process middleware after acquiring the invoker, but before invoking the method.
            foreach (var m in _middleware)
            {
                await m.HandleAsync(ct);
            }

            var result = await invoker.InvokeAsync(request.Params, ct);
            return new JsonRpcResponseSuccess
            {
                Id = request.Id,
                Result = JsonSerializer.SerializeToElement(result, _jsonOpt),
            }.ToResponse();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON error");
            throw new JsonRpcErrorException((int)JsonRpcErrorCode.ParseError, "JSON parse error");
        }
    }
}
