namespace JsonRpcX.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddScopedInterfaces<T>(this IServiceCollection services) =>
        services.AddSingletonInterfaces(typeof(T));

    public static IServiceCollection AddScopedInterfaces(this IServiceCollection services, Type type) =>
        services.AddInterfaces(ServiceLifetime.Scoped, type);

    public static IServiceCollection AddSingletonInterfaces<T>(this IServiceCollection services) =>
        services.AddSingletonInterfaces(typeof(T));

    public static IServiceCollection AddSingletonInterfaces(this IServiceCollection services, Type type) =>
        services.AddInterfaces(ServiceLifetime.Singleton, type);

    private static IServiceCollection AddInterfaces(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        Type type
    )
    {
        foreach (var i in type.GetInterfaces())
        {
            services.Add(new ServiceDescriptor(i, null, type, lifetime));
        }

        return services;
    }
}
