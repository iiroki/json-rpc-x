using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;

namespace JsonRpcX.Api.Middleware;

public class JsonRpcExampleExceptionHandler(ILogger<JsonRpcExampleExceptionHandler> logger) : IJsonRpcExceptionHandler
{
    private readonly ILogger _logger = logger;

    public Task<JsonRpcError?> HandleAsync(JsonRpcContext ctx, Exception ex, CancellationToken ct = default)
    {
        _logger.LogInformation("Custom JSON RPC error handler triggered");
        return Task.FromResult<JsonRpcError?>(null);
    }
}
