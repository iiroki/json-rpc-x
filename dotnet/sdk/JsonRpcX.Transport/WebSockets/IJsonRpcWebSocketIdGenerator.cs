using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Transport.WebSockets;

public interface IJsonRpcWebSocketIdGenerator
{
    string Generate(HttpContext ctx);
}
