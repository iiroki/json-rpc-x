using System.Net.WebSockets;

namespace JsonRpcX.WebSockets;

internal interface IJsonRpcWebSocketProcessor
{
    Task AttachAsync(WebSocket ws, HttpContext ctx, CancellationToken ct = default);
}
