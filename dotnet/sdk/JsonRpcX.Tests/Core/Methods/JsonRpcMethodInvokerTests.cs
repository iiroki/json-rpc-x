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
    public async Task Invoke_MultipleParams_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.MultipleParams));
        var @params = JsonSerializer.SerializeToElement(new List<int> { 1, 2 });

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1, 2, 3)]
    public async Task Invoke_MultipleParams_IncorrectCount_Exception(params int[] items)
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.MultipleParams));
        var @params = JsonSerializer.SerializeToElement(items);

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);
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

    private static JsonRpcMethodInvoker CreateMethodInvoker(string name)
    {
        var handler = new TestJsonRpcApi();
        var method = handler.GetType().GetMethod(name);
        return method != null
            ? new JsonRpcMethodInvoker(handler, method)
            : throw new ArgumentException($"Method to test not found: {name}");
    }

    private class TestJsonRpcApi : IJsonRpcMethodHandler
    {
        //
        // Params
        //

        [JsonRpcMethod]
        public static void MultipleParams(int a, int b)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ObjectPayload(TestObject obj)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void MixedPayload(string name, TestObject obj, IEnumerable<string> items)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void EnumerablePayload(IEnumerable<int> items)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static async Task AsyncCt(string _, CancellationToken ct) => await Task.Delay(10, ct);

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
