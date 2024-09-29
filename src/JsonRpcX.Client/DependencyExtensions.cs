using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Client;

public static class DependencyExtensions
{
    /// <summary>
    /// Adds JSON RPC client services.
    /// </summary>
    public static IServiceCollection AddJsonRpcClient(this IServiceCollection services) =>
        services
            .AddSingleton<IJsonRpcRequestAwaiter, JsonRpcRequestAwaiter>()
            .AddSingleton<IJsonRpcClientManager, JsonRpcClientManager>()
            .AddSingleton<IJsonRpcClientContainer>(sp => sp.GetRequiredService<IJsonRpcClientManager>());
}
