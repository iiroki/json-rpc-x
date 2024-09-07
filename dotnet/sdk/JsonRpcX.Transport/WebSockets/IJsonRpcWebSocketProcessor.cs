using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Transport.WebSockets;

internal interface IJsonRpcWebSocketProcessor
{
    Task AttachAsync(WebSocket ws, HttpContext ctx, CancellationToken ct = default);
}
