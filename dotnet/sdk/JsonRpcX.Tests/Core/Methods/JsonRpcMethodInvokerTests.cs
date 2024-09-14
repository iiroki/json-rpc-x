using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Core.Methods;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Helpers.Constants;
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
    public async Task Invoke_Params_Object_InvalidNull_Exception()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsObject));
        var @params = JsonConstants.Null;

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
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

    [Fact]
    public async Task Invoke_Params_Enumerable_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsEnumerable));
        var @params = JsonSerializer.SerializeToElement(Enumerable.Range(0, 1000));

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Fact]
    public async Task Invoke_Params_Enumerable_InvalidType_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsEnumerable));
        var @params = JsonSerializer.SerializeToElement(new List<string> { "1st", "2nd", "3rd", "4th" });

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    [Fact]
    public async Task Invoke_Params_Mixed_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsMixed));
        var @params = JsonSerializer.SerializeToElement(
            new List<object>
            {
                "Random Name",
                new TestObject { Id = 321, Name = "Test Object" },
                new List<string> { "1st", "2nd", "3rd", "4th" },
            }
        );

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Fact]
    public async Task Invoke_Params_Mixed_Invalid_Exception()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsMixed));
        var @params = JsonSerializer.SerializeToElement(
            new List<object>
            {
                new List<string> { "1st", "2nd", "3rd", "4th" },
                "Random Name",
                new TestObject { Id = 321, Name = "Test Object" },
            }
        );

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    [Fact]
    public async Task Invoke_Params_AsyncCt_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsAsyncCt));
        var @params = JsonSerializer.SerializeToElement("param");

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Fact]
    public async Task Invoke_Params_EnumerableAsyncCt_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsEnumerableAsyncCt));
        var @params = JsonSerializer.SerializeToElement(Enumerable.Range(0, 1000));

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Invoke_Params_NullReference_Valid_Ok(bool isJsonNull)
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsNullReference));
        JsonElement? @params = isJsonNull ? JsonConstants.Null : null;

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Invoke_Params_NullValue_Valid_Ok(bool isJsonNull)
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsNullValue));
        JsonElement? @params = isJsonNull ? JsonSerializer.SerializeToElement((int?)null) : null;

        // Act
        await invoker.InvokeAsync(@params);
    }

    [Fact]
    public async Task Invoke_Params_Default_Valid_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsDefault));
        var @params = JsonSerializer.SerializeToElement(new List<int> { 1 });

        // Act
        var actual = await invoker.InvokeAsync(@params);

        // Assert
        Assert.Equal(123, actual);
    }

    [Fact]
    public async Task Invoke_Params_Default_InvalidCount_Exception()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsDefault));
        var @params = JsonConstants.Null;

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Invoke_Params_DefaultMultiple_Valid_Ok(bool isJsonNull)
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsDefaultMultiple));
        JsonElement? @params = isJsonNull ? JsonSerializer.SerializeToElement((int?)null) : null;

        // Act
        var actual = await invoker.InvokeAsync(@params);

        // Assert
        Assert.Equal(3, actual);
    }

    [Fact]
    public async Task Invoke_Params_InvalidType_Exception()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsMultiple));
        var @params = JsonSerializer.SerializeToElement("invalid");

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    [Fact]
    public async Task Invoke_Params_Undefined_Exception()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ParamsMultiple));
        var @params = JsonConstants.Undefined;

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(@params);

        // Assert
        await Assert.ThrowsAsync<JsonRpcParamException>(fn);
    }

    #endregion

    #region Result

    [Fact]
    public async Task Invoke_Result_HelloWorld_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ResultHelloWorld));

        // Act
        var actual = await invoker.InvokeAsync(null);

        // Assert
        Assert.Equal("Hello, World!", actual);
    }

    [Fact]
    public async Task Invoke_Result_ObjectAsync_Ok()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ResultObjectAsync));
        var expected = new TestObject { Id = 123, Name = nameof(Invoke_Result_ObjectAsync_Ok) };
        var @params = JsonSerializer.SerializeToElement(new List<object> { expected.Id, expected.Name });

        // Act
        var actual = await invoker.InvokeAsync(@params);

        // Assert
        Assert.Equal(expected, actual);
    }

    #endregion

    #region Throw

    [Fact]
    public async Task Invoke_Throw_InnerException()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.Throw));

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(null);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(fn);
    }

    [Fact]
    public async Task Invoke_ThrowAsync_InnerException()
    {
        // Arrange
        var invoker = CreateMethodInvoker(nameof(TestJsonRpcApi.ThrowAsync));

        // Act
        async Task<object?> fn() => await invoker.InvokeAsync(null);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(fn);
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
        public static void ParamsMultiple(int _1, int _2)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsObject(TestObject _)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsEnumerable(IEnumerable<int> _)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsMixed(string _1, TestObject _2, IEnumerable<string> _3)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static async Task ParamsAsyncCt(string _, CancellationToken ct) => await Task.Delay(10, ct);

        [JsonRpcMethod]
        public static async Task ParamsEnumerableAsyncCt(IEnumerable<int> _, CancellationToken ct) =>
            await Task.Delay(10, ct);

        [JsonRpcMethod]
        public static void ParamsNullReference(object? _)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static void ParamsNullValue(int? _)
        {
            // NOP
        }

        [JsonRpcMethod]
        public static int ParamsDefault(int _, int @default = 123) => @default;

        [JsonRpcMethod]
        public static int ParamsDefaultMultiple(int default1 = 1, int default2 = 2) => default1 + default2;

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

        //
        // Throw
        //

        [JsonRpcMethod]
        public static string Throw() => throw new InvalidOperationException("Simulated failure");

        [JsonRpcMethod]
        public static async Task ThrowAsync(CancellationToken ct)
        {
            await Task.Delay(10, ct);
            throw new InvalidOperationException("Simulated async failure");
        }
    }

    private record TestObject
    {
        public required long Id { get; init; }

        public required string Name { get; init; }
    }
}
