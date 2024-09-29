using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JsonRpcX.Domain.Models;
using JsonRpcX.Transport;

namespace JsonRpcX.Tests.Transport;

public sealed class JsonRpcHttpTransportTests : JsonRpcTransportTestBase
{
    private readonly HttpClient _client = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Http_Status_Ok(bool isNotification)
    {
        // Arrange
        App = CreateTestApp();

        App.MapJsonRpcHttp(TestRoute);
        _ = App.RunAsync();

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
        App = CreateTestApp();

        App.MapJsonRpcHttp(TestRoute);
        _ = App.RunAsync();

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
        App = CreateTestApp();

        App.MapJsonRpcHttp(TestRoute);
        _ = App.RunAsync();

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
        App = CreateTestApp();

        App.MapJsonRpcHttp(TestRoute);
        _ = App.RunAsync();

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
        App = CreateTestApp();

        App.MapJsonRpcHttp(TestRoute);
        _ = App.RunAsync();

        var content = CreateTestContent(new JsonRpcRequest { Id = "abc", Method = nameof(TestJsonRpcApi.Error) });

        // Act
        var res = await _client.PostAsync(TestEndpoint, content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
    }

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
}
