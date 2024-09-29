using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Tests.Helpers;

public static class JsonRpcTestHelper
{
    public static IServiceCollection CreateTestServices(IEnumerable<Type>? controllers = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJsonRpc();

        foreach (var h in controllers ?? [])
        {
            services.AddJsonRpcController(h);
        }

        return services;
    }
}
