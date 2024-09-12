using System.Text.Json;
using JsonRpcX.Client;

namespace JsonRpcX.Api.Services;

public class JsonRpcStatusWorker(IJsonRpcClientContainer clientContainer, ILogger<JsonRpcStatusWorker> logger)
    : BackgroundService
{
    private readonly IJsonRpcClientContainer _clientContainer = clientContainer;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var counter = 1;
        while (!ct.IsCancellationRequested)
        {
            foreach (var client in _clientContainer.Clients)
            {
                var res = await client.SendRequestAsync("status", new { counter });
                _logger.LogInformation("Received response: {R}", JsonSerializer.Serialize(res));
            }

            ++counter;
            await Task.Delay(10000, ct);
        }
    }
}
