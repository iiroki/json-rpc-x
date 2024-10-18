using System.Net.WebSockets;
using System.Text.Json;
using JsonRpcX.Client;
using JsonRpcX.Domain;
using JsonRpcX.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Transport.WebSockets;

internal class JsonRpcWebSocketProcessor(
    IJsonRpcProcessor<byte[], byte[]> messageProcessor,
    IJsonRpcRequestAwaiter requestAwaiter,
    IJsonRpcClientManager clientManager,
    IHostApplicationLifetime lifeTime,
    JsonSerializerOptions? jsonOpt = null
) : IJsonRpcWebSocketProcessor
{
    private readonly IJsonRpcProcessor<byte[], byte[]> _messageProcessor = messageProcessor;
    private readonly IJsonRpcRequestAwaiter _requestAwaiter = requestAwaiter;
    private readonly IJsonRpcClientManager _clientManager = clientManager;
    private readonly IHostApplicationLifetime _lifetime = lifeTime;
    private readonly JsonSerializerOptions? _jsonOpt = jsonOpt;

    public async Task AttachAsync(WebSocket ws, HttpContext ctx)
    {
        using var ctSource = CancellationTokenSource.CreateLinkedTokenSource(
            _lifetime.ApplicationStopping,
            ctx.RequestAborted
        );

        var ct = ctSource.Token;

        var id = Guid.NewGuid().ToString();
        _clientManager.Add(new JsonRpcWebSocketClient(id, _requestAwaiter, ws, ctx.User, _jsonOpt));

        try
        {
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

            await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, ct);
        }
        catch (OperationCanceledException)
        {
            // NOP
        }

        _clientManager.Remove(id);
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
