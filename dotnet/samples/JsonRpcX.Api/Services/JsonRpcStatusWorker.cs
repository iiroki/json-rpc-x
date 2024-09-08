using JsonRpcX.Transport.WebSockets;

namespace JsonRpcX.Api.Services;

public class JsonRpcStatusWorker(IJsonRpcWebSocketContainer wsContainer, ILogger<JsonRpcStatusWorker> logger)
    : BackgroundService
{
    private readonly IJsonRpcWebSocketContainer _wsContainer = wsContainer;
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
