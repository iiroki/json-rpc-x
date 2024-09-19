using JsonRpcX.Client;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Tests.Client;

public class DependencyExtensionsTests
{
    [Fact]
    public void AddJsonRpcClient_Ok()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddJsonRpcClient();
        var sp = services.BuildServiceProvider();

        // Assert
        sp.GetRequiredService<IJsonRpcRequestAwaiter>();
        sp.GetRequiredService<IJsonRpcClientManager>();
        sp.GetRequiredService<IJsonRpcClientContainer>();
    }
}
