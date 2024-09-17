using System.Security.Claims;
using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Client;
using JsonRpcX.Core;
using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Models;
using JsonRpcX.Methods;
using JsonRpcX.Tests.Helpers;
using JsonRpcX.Transport.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Tests.Core;

public class JsonRpcProcessorTests
{
    private readonly IServiceProvider _services = JsonRpcTestHelper
        .CreateTestServices([typeof(TestJsonRpcApi)])
        .BuildServiceProvider();

    [Fact]
    public async Task Process_Request_Ok()
    {
        // Arrange
        var processor = CreateProcessor<JsonRpcRequest>();
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = new JsonRpcRequest { Id = Guid.NewGuid().ToString(), Method = nameof(TestJsonRpcApi.HelloWorld) };

        // Act
        var res = await processor.ProcessAsync(req, ctx);

        // Assert
        Assert.NotNull(res);
        Assert.True(res.IsSuccess);
        Assert.Equal(req.Id, res.Success.Id);
        Assert.Equal("Hello, World!", res.Success.Result);
    }

    [Fact]
    public async Task Process_Notification_Ok()
    {
        // Arrange
        var processor = CreateProcessor<JsonRpcRequest>();
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = new JsonRpcRequest { Id = null, Method = nameof(TestJsonRpcApi.HelloWorld) };

        // Act
        var res = await processor.ProcessAsync(req, ctx);

        // Assert
        Assert.Null(res);
    }

    [Fact]
    public async Task Process_ParseError_Error()
    {
        // "Parse error" =
        //     - Invalid JSON was received by the server.
        //     - An error occurred on the server while parsing the JSON text.

        // Arrange
        var processor = CreateProcessor<byte[]>();
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };

        // Act
        var res = await processor.ProcessAsync([1, 2, 3], ctx);

        // Assert
        Assert.NotNull(res);
        Assert.False(res.IsSuccess);
        Assert.Null(res.Id);
        Assert.Equal((int)JsonRpcErrorCode.ParseError, res.Error.Error.Code);
    }

    [Fact]
    public async Task Process_InvalidRequest_Error()
    {
        // "Invalid Request" = The JSON sent is not a valid Request object.

        // Arrange
        var processor = CreateProcessor<byte[]>();
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = JsonSerializer.SerializeToUtf8Bytes(new { Unknown = true });

        // Act
        var res = await processor.ProcessAsync(req, ctx);

        // Assert
        Assert.NotNull(res);
        Assert.False(res.IsSuccess);
        Assert.Null(res.Id);
        Assert.Equal((int)JsonRpcErrorCode.InvalidRequest, res.Error.Error.Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Process_MethodNotFound_Error(bool IsNotification)
    {
        // Arrange
        var processor = CreateProcessor<JsonRpcRequest>();
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = new JsonRpcRequest { Id = IsNotification ? null : Guid.NewGuid().ToString(), Method = "notFound" };

        // Act
        var res = await processor.ProcessAsync(req, ctx);

        // Assert
        if (IsNotification)
        {
            // "Method not found" errors are not returned for notifications
            Assert.Null(res);
        }
        else
        {
            Assert.NotNull(res);
            Assert.False(res.IsSuccess);
            Assert.Equal(req.Id, res.Id);
            Assert.Equal((int)JsonRpcErrorCode.MethodNotFound, res.Error.Error.Code);
        }
    }

    private JsonRpcProcessor<T, JsonRpcResponse> CreateProcessor<T>() =>
        new(
            _services.GetRequiredService<IServiceScopeFactory>(),
            _services.GetRequiredService<IJsonRpcMessageParser<T>>(),
            _services.GetRequiredService<IJsonRpcRequestAwaiter>(),
            _services.GetRequiredService<IJsonRpcResponseSerializer<JsonRpcResponse>>(),
            _services.GetRequiredService<ILogger<JsonRpcProcessor<T, JsonRpcResponse>>>()
        );

    private class TestJsonRpcApi : IJsonRpcMethodHandler
    {
        [JsonRpcMethod]
        public static bool True() => true;

        [JsonRpcMethod]
        public static bool False() => false;

        [JsonRpcMethod]
        public static int One() => 1;

        [JsonRpcMethod]
        public static int Two() => 2;

        [JsonRpcMethod]
        public static string HelloWorld() => "Hello, World!";
    }
}
