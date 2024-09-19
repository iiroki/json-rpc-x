using System.Security.Claims;
using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Client;
using JsonRpcX.Core;
using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Methods;
using JsonRpcX.Middleware;
using JsonRpcX.Tests.Helpers;
using JsonRpcX.Transport.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonRpcX.Tests.Core;

public class JsonRpcProcessorTests
{
    private readonly IServiceCollection _services = JsonRpcTestHelper.CreateTestServices([typeof(TestJsonRpcApi)]);

    private IServiceProvider? _serviceProvider;

    private IServiceProvider ServiceProvider => _serviceProvider ??= _services.BuildServiceProvider();

    #region Request

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
        Assert.Equal(JsonSerializer.SerializeToElement("Hello, World!").Stringify(), res.Success.Result.Stringify());
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

    #endregion

    #region Response

    [Fact]
    public async Task Process_Response_Ok()
    {
        // Arrange
        var processor = CreateProcessor<byte[]>();
        var requestAwaiter = ServiceProvider.GetRequiredService<IJsonRpcRequestAwaiter>();

        var ctx = new JsonRpcContext
        {
            Transport = "TEST",
            User = new ClaimsPrincipal(),
            ClientId = Guid.NewGuid().ToString(),
        };

        var expected = new JsonRpcResponseSuccess
        {
            Id = Guid.NewGuid().ToString(),
            Result = JsonSerializer.SerializeToElement(new { IsTestResponse = true, Value = 1.23 }),
        };

        // Act
        var task = requestAwaiter.WaitForResponseAsync(ctx.ClientId, expected.Id, TimeSpan.FromSeconds(10));
        var output = await processor.ProcessAsync(JsonSerializer.SerializeToUtf8Bytes(expected.ToResponse()), ctx);
        var actual = await task;

        // Assert
        Assert.Null(output);
        Assert.True(actual.IsSuccess);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Result.Stringify(), actual.Success.Result.Stringify());
    }

    #endregion

    #region Error

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

    [Fact]
    public async Task Process_ErrorHandler_Invoked()
    {
        // Arrange
        ICollection<Exception> invocations = [];

        var error = new JsonRpcError { Code = 123, Message = "Custom Test Error" };
        JsonRpcError? errorFn(Exception ex) => error;

        _services.AddJsonRpcMiddleware<TestJsonRpcMiddleware>();
        _services.AddSingleton(invocations);
        _services.AddSingleton(errorFn);
        _services.SetJsonRpcExceptionHandler<TestJsonRpcExceptionHandler>();

        var processor = CreateProcessor<string>();

        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = new JsonRpcRequest { Id = Guid.NewGuid().ToString(), Method = nameof(TestJsonRpcApi.True) };

        // Act
        var res = await processor.ProcessAsync(JsonSerializer.Serialize(req), ctx);

        // Assert
        Assert.NotNull(res);
        Assert.False(res.IsSuccess);
        Assert.Equal(error, res.Error.Error);
        Assert.Single(invocations);
    }

    [Fact]
    public async Task Process_UnknownError_Error()
    {
        // Arrange
        _services.AddJsonRpcMiddleware<TestJsonRpcMiddleware>();
        var processor = CreateProcessor<string>();

        var ctx = new JsonRpcContext { Transport = "TEST", User = new ClaimsPrincipal() };
        var req = new JsonRpcRequest { Id = Guid.NewGuid().ToString(), Method = nameof(TestJsonRpcApi.True) };

        // Act
        var res = await processor.ProcessAsync(JsonSerializer.Serialize(req), ctx);

        // Assert
        Assert.NotNull(res);
        Assert.False(res.IsSuccess);
        Assert.Equal((int)JsonRpcErrorCode.InternalError, res.Error.Error.Code);
    }

    #endregion

    private JsonRpcProcessor<T, JsonRpcResponse> CreateProcessor<T>() =>
        new(
            ServiceProvider.GetRequiredService<IServiceScopeFactory>(),
            ServiceProvider.GetRequiredService<IJsonRpcMessageParser<T>>(),
            ServiceProvider.GetRequiredService<IJsonRpcRequestAwaiter>(),
            ServiceProvider.GetRequiredService<IJsonRpcResponseSerializer<JsonRpcResponse>>(),
            ServiceProvider.GetRequiredService<ILogger<JsonRpcProcessor<T, JsonRpcResponse>>>()
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

        [JsonRpcMethod]
        public static void Throw() => throw new InvalidOperationException("Simulated failure");
    }

    private class TestJsonRpcExceptionHandler(
        ICollection<Exception> invocations,
        Func<Exception, JsonRpcError?> errorFn
    ) : IJsonRpcExceptionHandler
    {
        private readonly ICollection<Exception> _invocations = invocations;
        private readonly Func<Exception, JsonRpcError?> _errorFn = errorFn;

        public Task<JsonRpcError?> HandleAsync(Exception ex, CancellationToken ct = default)
        {
            _invocations.Add(ex);
            return Task.FromResult(_errorFn(ex));
        }
    }

    private class TestJsonRpcMiddleware : IJsonRpcMiddleware
    {
        public const string Message = nameof(TestJsonRpcMiddleware);

        public Task HandleAsync(CancellationToken ct = default) => throw new InvalidOperationException(Message);
    }
}
