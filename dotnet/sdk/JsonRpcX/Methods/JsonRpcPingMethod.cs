using JsonRpcX.Attributes;
using JsonRpcX.Models;

namespace JsonRpcX.Methods;

/// <summary>
/// Default "ping" JSON RPC method implementation.
/// </summary>
public class JsonRpcPing(JsonRpcContext ctx, ILogger<JsonRpcPing> logger) : IJsonRpcMethodHandler
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod("ping")]
    public void HandleAsync(string? test = null)
    {
        _logger.LogDebug("Ping - Transport: {T}", _ctx.Transport);
    }
}
