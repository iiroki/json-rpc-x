using JsonRpcX.Attributes;
using JsonRpcX.Client;
using JsonRpcX.Domain.Models;
using JsonRpcX.Methods;

namespace JsonRpcX.Api.Methods;

public class JsonRpcNotifyMethods(
    JsonRpcContext ctx,
    IJsonRpcClientContainer clientContainer,
    ILogger<JsonRpcNotifyMethods> logger
) : IJsonRpcMethodHandler
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly IJsonRpcClientContainer _clientContainer = clientContainer;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The method sends a notification to all other connected clients.
    /// </summary>
    [JsonRpcMethod]
    public async Task NotifyOthersAsync(string method, string message, CancellationToken ct = default)
    {
        // Get all clients excl. self and notify them
        var clients = _clientContainer.Except(_ctx).ToList();
        var tasks = clients.Select(c => c.SendNotificationAsync(method, new { message }, ct));
        await Task.WhenAll(tasks);
        _logger.LogInformation("Notified {C} client(s)", clients.Count);
    }
}
