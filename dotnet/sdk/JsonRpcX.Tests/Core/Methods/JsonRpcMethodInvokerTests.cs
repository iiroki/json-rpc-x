using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Core.Methods;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Methods;

namespace JsonRpcX.Tests.Core.Methods;

public sealed class JsonRpcMethodInvokerTests
{
    #region Params

    [Fact]
    public async Task Invoke_Params_Multiple_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsMultiple));
        var @params = JsonSerializer.SerializeToElement(new List<int> { 1, 2 });

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1, 2, 3)]
    public async Task Invoke_Params_Multiple_InvalidCount_Exception(params int[] items)
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsMultiple));
        var @params = JsonSerializer.SerializeToElement(items);

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    [Fact]
    public async Task Invoke_Params_Object_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsObject));
        var @params = JsonSerializer.SerializeToElement(new TestObject { Id = 123, Name = nameof(TestObject) });

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Fact]
    public async Task Invoke_Params_Object_InvalidFields_Exception()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsObject));
        var @params = JsonSerializer.SerializeToElement(new { Key = "invalid" });

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    #endregion

    #region Result

    [Fact]
    public async Task Invoke_Result_Simple_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ResultHelloWorld));

        // Act
        var result = await invoker.InvokeAsync(null);

        // Assert
        Assert.Equal("Hello, World!", result);
    }

    #endregion

    #region Helpers

    private static JsonRpcMethodInvoker CreateMethodInvoker(string name)
    {
        var handler = new TestJsonRpcApi();
        var method = handler.GetType().GetMethod(name);
        return method != null
            ? new JsonRpcMethodInvoker(handler, method)
            : throw new ArgumentException($"Method to test not found: {name}");
    }

    #endregion

    private class TestJsonRpcApi : IJsonRpcMethodHandler
    {
        //
        // Params
        //

        [JsonRpcMethod]
        public static void ParamsMultiple(int a, int b)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsObject(TestObject obj)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsMixed(string name, TestObject obj, IEnumerable<string> items)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsEnumerable(IEnumerable<int> items)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static async Task ParamsAsyncCt(string _, CancellationToken ct) => await Task.Delay(10, ct);

        //
        // Result
        //

        [JsonRpcMethod]
        public static string ResultHelloWorld() => "Hello, World!";

        [JsonRpcMethod]
        public static TestObject ResultObject(long id, string name) => new() { Id = id, Name = name };

        [JsonRpcMethod]
        public static async Task<TestObject> ResultObjectAsync(long id, string name)
        {
            await Task.Delay(10);
            return ResultObject(id, name);
        }
    }

    private class TestObject
    {
        public required long Id { get; init; }

        public required string Name { get; init; }
    }
}
