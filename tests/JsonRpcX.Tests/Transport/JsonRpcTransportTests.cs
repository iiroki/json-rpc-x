using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Domain.Models;
using JsonRpcX.Methods;
using JsonRpcX.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Tests.Transport;

public sealed class JsonRpcTransportTests : IDisposable
{
    private const string TestUrl = "http://localhost:65431";
    private const string TestRoute = "/json-rpc-test";
    private const string TestEndpoint = TestUrl + TestRoute;

    private readonly HttpClient _client = new();
    private WebApplication? _app;

    public void Dispose()
    {
        var task = _app?.DisposeAsync();
        if (task.HasValue)
        {
            SpinWait.SpinUntil(() => task.Value.IsCompleted, TimeSpan.FromSeconds(5));
        }
    }

    #region Http

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Http_Status_Ok(bool isNotification)
    {
        // Arrange
        _app = CreateTestApp();

        _app.MapJsonRpcHttp(TestRoute);
        _ = _app.RunAsync();

        var content = CreateTestContent(
            new JsonRpcRequest { Id = isNotification ? null : "test", Method = nameof(TestJsonRpcApi.Method) }
        );

        // Act
        var res = await _client.PostAsync(TestEndpoint, content);

        // Assert
        Assert.Equal(isNotification ? HttpStatusCode.NoContent : HttpStatusCode.OK, res.StatusCode);
    }

    [Theory]
    [InlineData(true, "application/json-rpc")]
    [InlineData(true, "application/json")]
    [InlineData(true, "application/jsonrequest")]
    [InlineData(false, "text/plain")]
    [InlineData(false, "application/xml")]
    public async Task Http_Header_ContentType_Ok(bool isValid, string contentType)
    {
        // Arrange
        _app = CreateTestApp();

        _app.MapJsonRpcHttp(TestRoute);
        _ = _app.RunAsync();

        var content = CreateTestContent(
            new JsonRpcRequest { Id = "test", Method = nameof(TestJsonRpcApi.Method) },
            contentType
        );

        // Act
        var res = await _client.PostAsync(TestEndpoint, content);

        // Assert
        Assert.Equal(isValid ? HttpStatusCode.OK : HttpStatusCode.UnsupportedMediaType, res.StatusCode);
    }

    [Fact]
    public async Task Http_Header_ContentLength_Missing_Ok()
    {
        // Arrange
        _app = CreateTestApp();

        _app.MapJsonRpcHttp(TestRoute);
        _ = _app.RunAsync();

        var content = CreateTestContent(new JsonRpcRequest { Id = "test", Method = nameof(TestJsonRpcApi.Method) });
        content.Headers.ContentLength = null;

        // Act
        var res = await _client.PostAsync(TestEndpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.LengthRequired, res.StatusCode);
    }

    [Theory]
    [InlineData(true, "*/*")]
    [InlineData(true, "application/json-rpc")]
    [InlineData(true, "application/json")]
    [InlineData(true, "application/jsonrequest")]
    [InlineData(false, "application/xml")]
    [InlineData(false, "test/plain")]
    public async Task Http_Header_Accept_Ok(bool isValid, string contentType)
    {
        // Arrange
        _app = CreateTestApp();

        _app.MapJsonRpcHttp(TestRoute);
        _ = _app.RunAsync();

        var content = CreateTestContent(
            new JsonRpcRequest { Id = Guid.NewGuid().ToString(), Method = nameof(TestJsonRpcApi.Method) }
        );

        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

        // Act
        var res = await _client.PostAsync(TestEndpoint, content);

        // Assert
        Assert.Equal(isValid ? HttpStatusCode.OK : HttpStatusCode.NotAcceptable, res.StatusCode);
    }

    [Fact]
    public async Task Http_Error_Ok()
    {
        // Arrange
        _app = CreateTestApp();

        _app.MapJsonRpcHttp(TestRoute);
        _ = _app.RunAsync();

        var content = CreateTestContent(new JsonRpcRequest { Id = "abc", Method = nameof(TestJsonRpcApi.Error) });

        // Act
        var res = await _client.PostAsync(TestEndpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
    }

    #endregion

    #region WebSocket

    // TODO

    #endregion

    private static JsonContent CreateTestContent(JsonRpcRequest req, string? contentType = null)
    {
        var content = JsonContent.Create(req);
        if (!string.IsNullOrEmpty(contentType))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        content.Headers.ContentLength = content.ReadAsStream().Length;
        return content;
    }

    private static WebApplication CreateTestApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(TestUrl);
        builder.Services.AddJsonRpc().AddJsonRpcMethodHandler<TestJsonRpcApi>();
        builder.Logging.AddFilter(null, LogLevel.None);
        return builder.Build();
    }

    private class TestJsonRpcApi : IJsonRpcMethodHandler
    {
        [JsonRpcMethod]
        public static string Method() => "Hello";

        [JsonRpcMethod]
        public static void Error() => throw new InvalidOperationException();
    }
}
