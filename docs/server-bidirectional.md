# JSON RPC X - Server: Bidirectional

Bidirectional communication is only available for transports that support it!

## Access bidirectional JSON RPC clients

When a bidirectional transport is used, `IJsonRpcClient` instances are created from it
and added to `IJsonRpcClientContainer`.

To access the clients, just inject `IJsonRpcClientContainer` to a controller or middleware.

```cs
public class JsonRpcExampleMethods(
    JsonRpcContext ctx,
    IJsonRpcClientContainer clientContainer,
    ILogger<JsonRpcNotifyMethods> logger
) : IJsonRpcController
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly IJsonRpcClientContainer _clientContainer = clientContainer;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod]
    public async Task NotifyOthersAsync(string message, CancellationToken ct = default)
    {
        // Gets all bidirectional clients (except self) and notifies them
        var clients = _clientContainer.Except(_ctx).ToList();
        var tasks = clients.Select(c => c.SendNotificationAsync("notify", new { message }, ct));
        await Task.WhenAll(tasks);
    }
}
```
