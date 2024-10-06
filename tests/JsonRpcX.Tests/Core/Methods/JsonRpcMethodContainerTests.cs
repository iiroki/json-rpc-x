using System.Reflection;
using JsonRpcX.Methods;

namespace JsonRpcX.Tests.Core.Methods;

public class JsonRpcMethodContainerTests
{
    private static readonly MethodInfo TestMethod = typeof(JsonRpcMethodContainerTests).GetMethod(nameof(Dummy))!;

    [Fact]
    public void Methods_Builders_Ok()
    {
        // Arrange
        var builder = new JsonRpcMethodBuilder
        {
            Methods =
            [
                new() { Name = "method1", Metadata = TestMethod },
                new() { Name = "method2", Metadata = TestMethod },
                new() { Name = "method3", Metadata = TestMethod },
                new() { Name = "method4", Metadata = TestMethod },
                new() { Name = "method5", Metadata = TestMethod },
            ],
        };

        var expected = builder.Methods.Select(b => b.Name).Order().ToList();

        // Act
        var container = new JsonRpcMethodContainer(builder);

        // Assert
        Assert.Equal(expected, container.Methods.Keys.Order());
    }

    private static void Dummy() => throw new NotImplementedException();
}
