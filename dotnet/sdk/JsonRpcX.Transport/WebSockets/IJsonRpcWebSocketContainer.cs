using System.Net.WebSockets;

namespace JsonRpcX.Transport.WebSockets;

public interface IJsonRpcWebSocketContainer
{
    int Count { get; }

    void Add(string id, WebSocket ws);

    List<(string, WebSocket)> Get();

    WebSocket Get(string id);

    void Remove(string id);
}
