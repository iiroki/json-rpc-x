namespace JsonRpcX.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSingletonInterfaces<T>(this IServiceCollection services) =>
        services.AddSingletonInterfaces(typeof(T));

    public static IServiceCollection AddSingletonInterfaces(this IServiceCollection services, Type type)
    {
        foreach (var i in type.GetInterfaces())
        {
            services.AddSingleton(i, type);
        }

        return services;
    }
}
