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
        List<JsonRpcMethodMetadataBuilder> builders =
        [
            new JsonRpcMethodMetadataBuilder
            {
                Methods = new Dictionary<string, MethodInfo>
                {
                    { "method1", TestMethod },
                    { "method3", TestMethod },
                    { "method2", TestMethod },
                },
            },
            new JsonRpcMethodMetadataBuilder
            {
                Methods = new Dictionary<string, MethodInfo>
                {
                    { "method4", TestMethod },
                    { "method5", TestMethod },
                    { "method6", TestMethod },
                },
            },
        ];

        var expected = builders.SelectMany(b => b.Methods.Keys).Order().ToList();

        // Act
        var container = new JsonRpcMethodContainer(builders);

        // Assert
        Assert.Equal(expected, container.Methods.Keys.Order());
    }

    private static void Dummy() => throw new NotImplementedException();
}
