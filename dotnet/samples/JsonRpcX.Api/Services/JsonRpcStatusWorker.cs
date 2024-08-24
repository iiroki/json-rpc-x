using JsonRpcX.Ws;

namespace JsonRpcX.Api.Services;

public class JsonRpcStatusWorker(IWebSocketContainer wsContainer, ILogger<JsonRpcStatusWorker> logger)
    : BackgroundService
{
    private readonly IWebSocketContainer _wsContainer = wsContainer;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            _logger.LogDebug("JSON RPC WebSocket count: {C}", _wsContainer.Count);
            await Task.Delay(10000, ct);
        }
    }
}
