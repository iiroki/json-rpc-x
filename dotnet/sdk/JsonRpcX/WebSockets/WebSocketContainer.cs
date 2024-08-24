using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace JsonRpcX.WebSockets;

internal class WebSocketContainer : IJsonRpcWebSocketContainer
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    public int Count => _sockets.Count;

    public void Add(string id, WebSocket ws)
    {
        if (!_sockets.TryAdd(id, ws))
        {
            throw new InvalidOperationException($"Could not add WebSocket with ID: {id}");
        }
    }

    public List<(string, WebSocket)> Get() => _sockets.Select(s => (s.Key, s.Value)).ToList();

    public WebSocket Get(string id) =>
        _sockets.TryGetValue(id, out var ws)
            ? ws
            : throw new InvalidOperationException($"WebSocket not found with ID: {id}");

    public void Remove(string id) => _sockets.Remove(id, out _);
}
