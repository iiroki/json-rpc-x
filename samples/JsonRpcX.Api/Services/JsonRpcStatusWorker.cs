using JsonRpcX.Client;

namespace JsonRpcX.Api.Services;

public class JsonRpcStatusWorker(IJsonRpcClientContainer clientContainer, ILogger<JsonRpcStatusWorker> logger)
    : BackgroundService
{
    private readonly IJsonRpcClientContainer _clientContainer = clientContainer;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            _logger.LogInformation("Online clients: {C}", _clientContainer.Clients.Count());
            await Task.Delay(10000, ct);
        }
    }
}
