using System.Net.WebSockets;
using System.Text.Json;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport;
using Microsoft.AspNetCore.Builder;

namespace JsonRpcX.Tests.Transport;

public class JsonRpcWebSocketTransportTests : JsonRpcTransportTestBase
{
    private const string TestRoute = "/json-rpc-test/ws";

    private readonly ClientWebSocket _client = new();

    protected string TestEndpoint => "ws://" + TestAppUrl + TestRoute;

    protected override void DisposeInternal() => _client.Dispose();

    public JsonRpcWebSocketTransportTests()
    {
        InitTestApp();
        TestApp.UseWebSockets();
        TestApp.MapJsonRpcWebSocket(TestRoute);
        _ = TestApp.RunAsync();
        ConnectAsync().Wait();
    }

    [Fact]
    public async Task WebSocket_Valid_Smoke_Ok()
    {
        // Arrange
        var req = new JsonRpcRequest { Id = "abc", Method = nameof(TestJsonRpcApi.Method) };
        var payload = JsonSerializer.SerializeToUtf8Bytes(req);

        // Act
        var task = WaitForDataAsync<JsonRpcResponse>();
        await _client.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
        var res = await task;

        // Assert
        Assert.NotNull(res);
        Assert.True(res.IsSuccess);
        Assert.Equal(req.Id, res.Id);
    }

    [Fact]
    public async Task WebSocket_Invalid_Smoke_Ok()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToUtf8Bytes(new { Invalid = true });

        // Act
        var task = WaitForDataAsync<JsonRpcResponse>();
        await _client.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
        var res = await task;

        // Assert
        Assert.NotNull(res);
        Assert.False(res.IsSuccess);
    }

    private async Task ConnectAsync() => await _client.ConnectAsync(new Uri(TestEndpoint), CancellationToken.None);

    private async Task<T?> WaitForDataAsync<T>(TimeSpan? timeout = null)
    {
        var ctSource = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(10));

        var buffer = new byte[4096];
        var result = await _client.ReceiveAsync(buffer, ctSource.Token);

        return JsonSerializer.Deserialize<T>(buffer[..result.Count]);
    }
}
