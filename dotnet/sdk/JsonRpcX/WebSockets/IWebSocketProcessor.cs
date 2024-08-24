using System.Net.WebSockets;

namespace JsonRpcX.Services;

internal interface IWebSocketProcessor
{
    Task AttachAsync(WebSocket ws, HttpContext ctx, CancellationToken ct = default);
}
