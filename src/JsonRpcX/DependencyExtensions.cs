using JsonRpcX.Attributes;
using JsonRpcX.Authorization;
using JsonRpcX.Client;
using JsonRpcX.Context;
using JsonRpcX.Controllers;
using JsonRpcX.Core.Schema;
using JsonRpcX.Domain;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Methods;
using JsonRpcX.Middleware;
using JsonRpcX.Requests;
using JsonRpcX.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JsonRpcX;

public static class DependencyExtensions
{
    /// <summary>
    /// Adds the base services for JSON RPC processing.
    /// </summary>
    public static IServiceCollection AddJsonRpc(this IServiceCollection services) =>
        services
            // Request services:
            .AddScoped<IJsonRpcContextManager, JsonRpcContextManager>()
            .AddScoped<IJsonRpcContextProvider>(sp => sp.GetRequiredService<IJsonRpcContextManager>())
            .AddScoped(sp => sp.GetRequiredService<IJsonRpcContextProvider>().Context)
            .AddScoped<IJsonRpcRequestHandler, JsonRpcRequestHandler>()
            .AddScoped<IJsonRpcMethodFactory, JsonRpcMethodFactory>()
            // Global services:
            .AddJsonRpcClient()
            .AddJsonRpcSerializerDefaults()
            .SetJsonRpcAuthorizationHandler<JsonRpcAuthorizationHandler>()
            .AddSingleton(typeof(IJsonRpcProcessor<,>), typeof(JsonRpcProcessor<,>))
            .AddSingleton<IJsonRpcMethodContainer, JsonRpcMethodContainer>();

    /// <summary>
    /// Adds the <c>IJsonRpcController </c> implementation to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcController(
        this IServiceCollection services,
        Type type,
        JsonRpcMethodOptions? opt = null
    )
    {
        if (!IsValidJsonRpcControllerType(type))
        {
            throw new ArgumentException($"\"{nameof(type)}\" is not valid \"{nameof(IJsonRpcController)}\" type");
        }

        List<JsonRpcMethodInfo> methods = [];
        foreach (var m in type.GetMethods())
        {
            var attr = (JsonRpcMethodAttribute?)
                m.GetCustomAttributes(typeof(JsonRpcMethodAttribute), true).FirstOrDefault();

            if (attr != null)
            {
                var auth = opt?.AuthorizationResolver?.Invoke(m) ?? JsonRpcAuthorizationResolver.GetAuthorization(m);
                var info = new JsonRpcMethodInfo
                {
                    Name =
                        opt?.NameResolver?.Invoke(m, attr)
                        ?? JsonRpcMethodNameResolver.GetName(m, attr, opt?.NamingPolicy),
                    Metadata = m,
                    Authorization = auth,
                };

                methods.Add(info);
                services.AddJsonRpcMethod(info.Name, type);
            }
        }

        if (methods.Count > 0)
        {
            services.AddSingleton(new JsonRpcMethodBuilder { Methods = methods });
        }

        return services;
    }

    /// <summary>
    /// Adds the <c>IJsonRpcController </c> implementation to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcController<T>(
        this IServiceCollection services,
        JsonRpcMethodOptions? opt = null
    )
        where T : IJsonRpcController => services.AddJsonRpcController(typeof(T), opt);

    /// <summary>
    /// Reads <c>IJsonRpcController </c> implementations from the current app domain's
    /// assemblies and adds them to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcControllersFromAssebly(
        this IServiceCollection services,
        JsonRpcMethodOptions? opt = null
    )
    {
        var controllerTypes = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(IsValidJsonRpcControllerType)
            .ToList();

        controllerTypes.ForEach(h => services.AddJsonRpcController(h, opt));
        return services;
    }

    /// <summary>
    /// Adds <c>IJsonRpcMiddleware</c> implementation to the services as a scoped service.
    /// </summary>
    public static IServiceCollection AddJsonRpcMiddleware(this IServiceCollection services, Type type) =>
        services.AddScoped(typeof(IJsonRpcMiddleware), type);

    /// <summary>
    /// Adds <c>IJsonRpcMiddleware</c> implementation to the services as a scoped service.
    /// </summary>
    public static IServiceCollection AddJsonRpcMiddleware<T>(this IServiceCollection services)
        where T : IJsonRpcMiddleware => services.AddJsonRpcMiddleware(typeof(T));

    /// <summary>
    /// Set the <c>IJsonRpcExceptionHandler</c> implementation used for error handling.<br />
    /// <br/>
    /// NOTE:<br/>
    /// Using this method REPLACES the exception handler, since there can only be one!
    /// </summary>
    public static IServiceCollection SetJsonRpcExceptionHandler<T>(this IServiceCollection services)
        where T : IJsonRpcExceptionHandler => services.SetJsonRpcExceptionHandler(typeof(T));

    /// <inheritdoc cref="SetJsonRpcExceptionHandler(IServiceCollection)" />
    public static IServiceCollection SetJsonRpcExceptionHandler(this IServiceCollection services, Type type) =>
        services.Replace(ServiceDescriptor.Scoped(typeof(IJsonRpcExceptionHandler), type));

    /// <summary>
    /// Set the <c>IJsonRpcAuthorizationHandler</c> implementation for authorization.<br />
    /// <br/>
    /// NOTE:<br/>
    /// Using this method REPLACES the authorization handler, since there can only be one!
    /// </summary>
    public static IServiceCollection SetJsonRpcAuthorizationHandler<T>(this IServiceCollection services)
        where T : IJsonRpcAuthorizationHandler => services.SetJsonRpcAuthorizationHandler(typeof(T));

    /// <inheritdoc cref="SetJsonRpcAuthorizationHandler(IServiceCollection)" />
    public static IServiceCollection SetJsonRpcAuthorizationHandler(this IServiceCollection services, Type type) =>
        services.Replace(ServiceDescriptor.Singleton(typeof(IJsonRpcAuthorizationHandler), type));

    /// <summary>
    /// Maps the JSON RPC API schema endpoint to the given route.
    /// </summary>
    public static WebApplication MapJsonRpcSchema(this WebApplication app, string route)
    {
        app.MapGet(route, new JsonRpcSchemaEndpointFactory().Create());
        return app;
    }

    private static bool IsValidJsonRpcControllerType(Type type) =>
        typeof(IJsonRpcController).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract;
}
