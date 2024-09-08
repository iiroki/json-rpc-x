using System.Net.WebSockets;
using JsonRpcX.Domain.Interfaces;
using JsonRpcX.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Transport.WebSockets;

internal class JsonRpcWebSocketProcessor(
    IJsonRpcProcessor<byte[], byte[]> messageProcessor,
    IJsonRpcWebSocketContainer container,
    IJsonRpcWebSocketIdGenerator idGenerator,
    ILogger<JsonRpcWebSocketProcessor> logger
) : IJsonRpcWebSocketProcessor
{
    private readonly IJsonRpcProcessor<byte[], byte[]> _messageProcessor = messageProcessor;
    private readonly IJsonRpcWebSocketContainer _container = container;
    private readonly IJsonRpcWebSocketIdGenerator _idGenerator = idGenerator;
    private readonly ILogger _logger = logger;

    public async Task AttachAsync(WebSocket ws, HttpContext ctx)
    {
        var ct = ctx.RequestAborted;

        var id = _idGenerator.Generate(ctx);
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

    private async Task HandleAsync(WebSocket ws, byte[] buffer, HttpContext http, CancellationToken ct)
    {
        var ctx = new JsonRpcContext { Http = http };
        var response = await _messageProcessor.ProcessAsync(buffer, ctx, ct);
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
