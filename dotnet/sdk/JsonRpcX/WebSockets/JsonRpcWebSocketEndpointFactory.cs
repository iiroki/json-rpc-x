using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using JsonRpcX.Core.Endpoints;
using JsonRpcX.Extensions;
using JsonRpcX.Models;
using JsonRpcX.Services;

namespace JsonRpcX.WebSockets;

internal class JsonRpcWebSocketEndpointFactory(bool shouldSendInitNotification) : IJsonRpcEndpointFactory
{
    private readonly bool _shouldSendInitNotification = shouldSendInitNotification;

    public RequestDelegate Create() =>
        async (ctx) =>
        {
            var services = ctx.RequestServices;
            var container = services.GetRequiredService<IJsonRpcWebSocketContainer>();
            var processor = services.GetRequiredService<IWebSocketProcessor>();
            var jsonOptions = services.GetService<JsonSerializerOptions>();

            if (ctx.WebSockets.IsWebSocketRequest)
            {
                using var ws = await ctx.WebSockets.AcceptWebSocketAsync();

                var task = processor.AttachAsync(ws, ctx);

                // Send the initial notification
                if (_shouldSendInitNotification)
                {
                    var notification = new JsonRpcRequest { Method = "init" };
                    var content = JsonSerializer.Serialize(notification, jsonOptions).GetUtf8Bytes();
                    await ws.SendAsync(content, WebSocketMessageType.Text, true, CancellationToken.None);
                }

                // Wait for the WebSocket processor to complete
                await task;
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        };
}
