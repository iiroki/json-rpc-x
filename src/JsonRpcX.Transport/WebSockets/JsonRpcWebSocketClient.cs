using System.Net.WebSockets;
using System.Security.Claims;
using System.Text.Json;
using JsonRpcX.Client;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.WebSockets;

internal class JsonRpcWebSocketClient(
    string id,
    IJsonRpcRequestAwaiter requestAwaiter,
    WebSocket ws,
    ClaimsPrincipal? user,
    JsonSerializerOptions? jsonOpt = null
) : IJsonRpcClient
{
    private readonly IJsonRpcRequestAwaiter _requestAwaiter = requestAwaiter;
    private readonly WebSocket _ws = ws;
    private readonly ClaimsPrincipal _user = user ?? new ClaimsPrincipal();
    private readonly JsonSerializerOptions? _jsonOpt = jsonOpt;

    public string Id { get; } = id;

    public string Transport { get; } = JsonRpcTransportType.WebSocket;

    public ClaimsPrincipal User => _user;

    public async Task<JsonRpcResponse> SendRequestAsync(string method, object? @params, TimeSpan? timeout = null)
    {
        var requestId = Guid.NewGuid().ToString();
        await SendAsync(method, requestId, @params);
        return await _requestAwaiter.WaitForResponseAsync(Id, requestId, timeout);
    }

    public async Task SendNotificationAsync(string method, object? @params, CancellationToken ct = default) =>
        await SendAsync(method, null, @params, ct);

    private async Task SendAsync(string method, string? id, object? @params, CancellationToken ct = default)
    {
        var request = new JsonRpcRequest
        {
            Id = id,
            Method = method,
            Params = @params != null ? JsonSerializer.SerializeToElement(@params, _jsonOpt) : null,
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(request, _jsonOpt);

        await _ws.SendAsync(data, WebSocketMessageType.Text, true, ct);
    }
}
