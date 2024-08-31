using JsonRpcX.Middleware;
using JsonRpcX.Models;

namespace JsonRpcX.Api.Middleware;

public class JsonRpcExampleMiddleware(JsonRpcContext ctx) : IJsonRpcMiddleware
{
    private readonly JsonRpcContext _ctx = ctx;

    public Task HandleAsync(CancellationToken ct = default)
    {
        _ctx.Data.Add("middleware", nameof(JsonRpcExampleMiddleware));
        return Task.CompletedTask;
    }
}
