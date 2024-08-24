using System.Net.WebSockets;

namespace JsonRpcX.Services;

public interface IWebSocketProcessor
{
    Task AttachAsync(WebSocket ws, HttpContext ctx, CancellationToken ct = default);
}
