using System.Net;
using System.Text.Json;
using JsonRpcX.Transport.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport.WebSockets;

internal class JsonRpcWebSocketTransport : IJsonRpcTransport
{
    public RequestDelegate Delegate { get; } =
        async ctx =>
        {
            var processor = ctx.RequestServices.GetRequiredService<IJsonRpcWebSocketProcessor>();
            if (ctx.WebSockets.IsWebSocketRequest)
            {
                using var ws = await ctx.WebSockets.AcceptWebSocketAsync();

                var task = processor.AttachAsync(ws, ctx);

                // Send the initial notification
                // if (_shouldSendInitNotification)
                // {
                //     var notification = new JsonRpcRequest { Method = "init" };
                //     var content = JsonSerializer.Serialize(notification, jsonOptions).GetUtf8Bytes();
                //     await ws.SendAsync(content, WebSocketMessageType.Text, true, ctx.RequestAborted);
                // }

                // Wait for the WebSocket processor to complete
                await task;
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        };
}
