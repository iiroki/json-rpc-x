using System.Net.WebSockets;
using JsonRpcX.Handlers;
using JsonRpcX.Ws;

namespace JsonRpcX.Services;

internal class WebSocketProcessor(
    IMessageHandler<byte[], byte[]?> handler,
    IWebSocketContainer container,
    ILogger<WebSocketProcessor> logger
) : IWebSocketProcessor
{
    private readonly IMessageHandler<byte[], byte[]?> _handler = handler;
    private readonly IWebSocketContainer _container = container;
    private readonly ILogger _logger = logger;

    // See "Echo":
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-8.0#send-and-receive-messages
    public async Task AttachAsync(WebSocket ws, HttpContext ctx, CancellationToken ct = default)
    {
        const string id = "TODO";
        _container.Add(id, ws);

        var buffer = new byte[1024 * 4];
        var result = await ws.ReceiveAsync(buffer, ct);

        if (!result.EndOfMessage)
        {
            throw new WebSocketException("TODO: Unhandled \"end of message\" = false");
        }

        while (!result.CloseStatus.HasValue)
        {
            result = await ws.ReceiveAsync(buffer, ct);

            // Process the messages in a non-blocking manner
            _ = HandleAsync(ws, buffer[..result.Count], ctx, ct);
        }

        _container.Remove(id);
        await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, ct);
    }

    private async Task HandleAsync(WebSocket ws, byte[] buffer, HttpContext ctx, CancellationToken ct)
    {
        var response = await _handler.HandleAsync(buffer, ctx, ct);
        if (response != null)
        {
            if (ws.CloseStatus.HasValue)
            {
                _logger.LogWarning("WebSocket was closed before sending response - Status: {S}", ws.CloseStatus);
                return;
            }

            await ws.SendAsync(response, WebSocketMessageType.Text, true, ct);
        }
    }
}
