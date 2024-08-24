using JsonRpcX.Attributes;
using JsonRpcX.Handlers;
using JsonRpcX.Models;
using JsonRpcX.Services;

namespace JsonRpcX.Methods;

public class JsonRpcPing(JsonRpcContext ctx, IJsonRpcContextProvider ctxProvider, ILogger<JsonRpcPing> logger)
    : IJsonRpcMethodHandler
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly IJsonRpcContextProvider _ctxProvider = ctxProvider;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod("ping")]
    public void HandleAsync(string? test = null)
    {
        _logger.LogDebug("!!! PING");
    }
}
