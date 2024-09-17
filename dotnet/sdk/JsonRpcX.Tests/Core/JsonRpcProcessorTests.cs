using System.Security.Claims;
using JsonRpcX.Attributes;
using JsonRpcX.Client;
using JsonRpcX.Core;
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
        var processor = CreateProcessor();
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
        var processor = CreateProcessor();
        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = new JsonRpcRequest { Id = null, Method = nameof(TestJsonRpcApi.HelloWorld) };

        // Act
        var res = await processor.ProcessAsync(req, ctx);

        // Assert
        Assert.Null(res);
    }

    private JsonRpcProcessor<JsonRpcRequest, JsonRpcResponse> CreateProcessor() =>
        new(
            _services.GetRequiredService<IServiceScopeFactory>(),
            _services.GetRequiredService<IJsonRpcMessageParser<JsonRpcRequest>>(),
            _services.GetRequiredService<IJsonRpcRequestAwaiter>(),
            _services.GetRequiredService<IJsonRpcResponseSerializer<JsonRpcResponse>>(),
            _services.GetRequiredService<ILogger<JsonRpcProcessor<JsonRpcRequest, JsonRpcResponse>>>()
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
