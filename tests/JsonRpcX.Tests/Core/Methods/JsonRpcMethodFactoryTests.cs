using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Controllers;
using JsonRpcX.Core.Methods;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Tests.Core.Methods;

public class JsonRpcMethodFactoryTests
{
    private readonly JsonRpcMethodFactory _factory = CreateTestFactory();

    [Theory]
    [InlineData(nameof(TestJsonRpcApi.True))]
    [InlineData(nameof(TestJsonRpcApi.False))]
    [InlineData(nameof(TestJsonRpcApi.One))]
    [InlineData(nameof(TestJsonRpcApi.Two))]
    [InlineData(nameof(TestJsonRpcApi.HelloWorld))]
    public void Create_Ok(string method)
    {
        // Act
        var invoker = _factory.Create(method);

        // Assert
        Assert.Equal(typeof(TestJsonRpcApi).FullName, invoker.Controller.GetType().FullName);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("method")]
    public void Create_Controller_NotFound_Exception(string method)
    {
        // Act
        IJsonRpcMethodInvoker fn() => _factory.Create(method);

        // Assert
        Assert.Throws<JsonRpcMethodNotFoundException>(fn);
    }

    [Fact]
    public void Create_Method_NotFound_Exception()
    {
        // Arrange
        var factory = new JsonRpcMethodFactory(
            CreateTestServices().BuildServiceProvider(),
            new TestJsonRpcMethodContainer(),
            null
        );

        // Act
        IJsonRpcMethodInvoker fn() => factory.Create(nameof(TestJsonRpcApi.HelloWorld));

        // Act
        Assert.Throws<JsonRpcException>(fn);
    }

    [Fact]
    public void Create_Method_TypeMismatch_Exception()
    {
        // Arrange
        var name = nameof(TestJsonRpcApi2.HelloWorld);

        var factory = new JsonRpcMethodFactory(
            CreateTestServices().BuildServiceProvider(),
            new TestJsonRpcMethodContainer([(name, typeof(TestJsonRpcApi2).GetMethod(name)!)]),
            null
        );

        // Act
        IJsonRpcMethodInvoker fn() => factory.Create(nameof(TestJsonRpcApi.HelloWorld));

        // Act
        Assert.Throws<JsonRpcException>(fn);
    }

    private static IServiceCollection CreateTestServices() =>
        JsonRpcTestHelper.CreateTestServices().AddJsonRpcController<TestJsonRpcApi>();

    private static JsonRpcMethodFactory CreateTestFactory()
    {
        var services = CreateTestServices();
        var sp = services.BuildServiceProvider();
        return new JsonRpcMethodFactory(
            sp,
            sp.GetRequiredService<IJsonRpcMethodContainer>(),
            sp.GetService<JsonSerializerOptions>()
        );
    }

    private class TestJsonRpcApi : IJsonRpcController
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

    private class TestJsonRpcApi2 : IJsonRpcController
    {
        [JsonRpcMethod]
        public static string HelloWorld() => throw new NotImplementedException();
    }

    private class TestJsonRpcMethodContainer(IEnumerable<(string, MethodInfo)>? methods = null)
        : IJsonRpcMethodContainer
    {
        public ImmutableDictionary<string, MethodInfo> Methods =>
            (methods ?? []).ToImmutableDictionary(p => p.Item1, p => p.Item2);
    }
}
