using Microsoft.AspNetCore.Http;

namespace JsonRpcX.WebSockets;

internal class JsonRpcWebSocketIdGenerator : IJsonRpcWebSocketIdGenerator
{
    public string Generate(HttpContext ctx) => ctx.Connection.Id;
}
