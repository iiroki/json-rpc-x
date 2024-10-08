using System.Net.WebSockets;
using System.Text.Json;
using JsonRpcX.Client;
using JsonRpcX.Domain;
using JsonRpcX.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Transport.WebSockets;

internal class JsonRpcWebSocketProcessor(
    IJsonRpcProcessor<byte[], byte[]> messageProcessor,
    IJsonRpcRequestAwaiter requestAwaiter,
    IJsonRpcClientManager clientManager,
    JsonSerializerOptions jsonOpt,
    ILogger<JsonRpcWebSocketProcessor> logger
) : IJsonRpcWebSocketProcessor
{
    private readonly IJsonRpcProcessor<byte[], byte[]> _messageProcessor = messageProcessor;
    private readonly IJsonRpcRequestAwaiter _requestAwaiter = requestAwaiter;
    private readonly IJsonRpcClientManager _clientManager = clientManager;

    private readonly JsonSerializerOptions _jsonOpt = jsonOpt;
    private readonly ILogger _logger = logger;

    public async Task AttachAsync(WebSocket ws, HttpContext ctx)
    {
        var ct = ctx.RequestAborted;
        ct.Register(() => _logger.LogInformation("WebSocket request cancelled - Status: {S}", ws.CloseStatus));

        var id = Guid.NewGuid().ToString();
        _clientManager.Add(new JsonRpcWebSocketClient(id, _requestAwaiter, ws, ctx.User, _jsonOpt));

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult? result = null;
        while (result == null || !result.CloseStatus.HasValue)
        {
            result = await ws.ReceiveAsync(buffer, ct);
            if (!result.EndOfMessage)
            {
                throw new WebSocketException("TODO: Unhandled \"end of message\" = false");
            }

            // Process the messages in a non-blocking manner
            _ = HandleAsync(id, ws, buffer[..result.Count], ctx, ct);
        }

        _clientManager.Remove(id);
        await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, ct);
    }

    private async Task HandleAsync(string clientId, WebSocket ws, byte[] buffer, HttpContext http, CancellationToken ct)
    {
        var ctx = new JsonRpcContext
        {
            Transport = JsonRpcTransportType.WebSocket,
            User = http.User,
            ClientId = clientId,
        };

        var response = await _messageProcessor.ProcessAsync(buffer, ctx, ct);
        if (response != null)
        {
            if (ws.CloseStatus.HasValue)
            {
                return;
            }

            await ws.SendAsync(response, WebSocketMessageType.Text, true, ct);
        }
    }
}
