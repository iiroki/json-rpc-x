using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddWithInterfaces<T>(this IServiceCollection services, ServiceLifetime lifetime) =>
        services.AddWithInterfaces(typeof(T), lifetime);

    public static IServiceCollection AddWithInterfaces(
        this IServiceCollection services,
        Type type,
        ServiceLifetime lifetime
    )
    {
        foreach (var i in type.GetInterfaces())
        {
            services.Add(new ServiceDescriptor(i, null, type, lifetime));
        }

        return services;
    }
}
