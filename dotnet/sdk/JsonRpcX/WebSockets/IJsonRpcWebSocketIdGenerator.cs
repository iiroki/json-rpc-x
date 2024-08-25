namespace JsonRpcX.WebSockets;

public interface IJsonRpcWebSocketIdGenerator
{
    string Generate(HttpContext ctx);
}
