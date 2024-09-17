using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Tests.Helpers;

public static class JsonRpcTestHelper
{
    public static IServiceCollection CreateTestServices(IEnumerable<Type>? handlers = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJsonRpc();

        foreach (var h in handlers ?? [])
        {
            services.AddJsonRpcMethodHandler(h);
        }

        return services;
    }
}
