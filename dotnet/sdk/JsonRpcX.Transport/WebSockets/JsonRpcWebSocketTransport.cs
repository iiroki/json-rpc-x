using System.Net;
using JsonRpcX.Transport.Constants;
using JsonRpcX.Transport.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport.WebSockets;

internal class JsonRpcWebSocketTransport : IJsonRpcTransport
{
    public string Type { get; } = JsonRpcTransportType.WebSocket;

    public RequestDelegate Delegate { get; } =
        async ctx =>
        {
            var processor = ctx.RequestServices.GetRequiredService<IJsonRpcWebSocketProcessor>();
            if (ctx.WebSockets.IsWebSocketRequest)
            {
                using var ws = await ctx.WebSockets.AcceptWebSocketAsync();
                var task = processor.AttachAsync(ws, ctx);

                // Wait for the WebSocket processor to complete
                await task;
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        };
}
