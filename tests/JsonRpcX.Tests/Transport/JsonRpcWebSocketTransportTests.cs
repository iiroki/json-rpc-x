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
    public async Task WebSocket_Todo()
    {
        // Arrange
        var req = new JsonRpcRequest { Id = "abc", Method = nameof(TestJsonRpcApi.Method) };
        var payload = JsonSerializer.SerializeToUtf8Bytes(req);

        // Act
        var ctSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var buffer = new byte[4096];
        var task = _client.ReceiveAsync(buffer, ctSource.Token);
        await _client.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);

        await task;
        var asd = JsonSerializer.Deserialize<JsonRpcResponse>(buffer);

        // Assert
        Assert.NotNull(asd);
        Assert.True(asd.IsSuccess);
        Assert.Equal(req.Id, asd.Success.Id);
    }

    private async Task ConnectAsync() => await _client.ConnectAsync(new Uri(TestEndpoint), CancellationToken.None);
}
