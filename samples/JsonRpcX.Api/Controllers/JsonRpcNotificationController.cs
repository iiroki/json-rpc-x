using JsonRpcX.Attributes;
using JsonRpcX.Client;
using JsonRpcX.Controllers;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Api.Controllers;

public class JsonRpcNotificationController(
    JsonRpcContext ctx,
    IJsonRpcClientContainer clientContainer,
    ILogger<JsonRpcNotificationController> logger
) : IJsonRpcController
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly IJsonRpcClientContainer _clientContainer = clientContainer;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The method sends a notification to all other connected clients.
    /// </summary>
    [JsonRpcMethod]
    public async Task NotifyOthersAsync(string message, CancellationToken ct = default)
    {
        // Get all clients excl. self and notify them
        var clients = _clientContainer.Except(_ctx).ToList();
        var tasks = clients.Select(c => c.SendNotificationAsync("notify", new { message }, ct));
        await Task.WhenAll(tasks);
        _logger.LogInformation("Notified {C} client(s)", clients.Count);
    }
}
