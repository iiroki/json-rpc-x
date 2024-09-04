using System.Reflection;
using JsonRpcX.Attributes;
using JsonRpcX.Core;
using JsonRpcX.Core.Context;
using JsonRpcX.Core.Exceptions;
using JsonRpcX.Core.Messages;
using JsonRpcX.Core.Methods;
using JsonRpcX.Core.Requests;
using JsonRpcX.Core.Schema;
using JsonRpcX.Core.Serialization;
using JsonRpcX.Exceptions;
using JsonRpcX.Extensions;
using JsonRpcX.Methods;
using JsonRpcX.Middleware;
using JsonRpcX.Options;
using JsonRpcX.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JsonRpcX;

public static class DependencyExtensions
{
    /// <summary>
    /// Adds the base services from JSON RPC processing.
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
            .AddWithInterfaces<JsonRpcSerializer>(ServiceLifetime.Singleton)
            .AddSingleton(typeof(IJsonRpcMessageProcessor<,>), typeof(JsonRpcMessageProcessor<,>))
            .AddSingleton<IJsonRpcMethodContainer, JsonRpcMethodContainer>()
            .AddSingleton<IJsonRpcExceptionHandler, JsonRpcExceptionHandler>()
            // WebSocket services:
            .AddJsonRpcWebSocket();

    // TODO: HTTP services

    /// <summary>
    /// Adds the <c>IJsonRpcMethodHandler </c> implementation to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcMethodHandler(
        this IServiceCollection services,
        Type type,
        JsonRpcMethodOptions? opt = null
    )
    {
        if (!IsValidJsonRpcMethodHandlerType(type))
        {
            throw new ArgumentException($"\"{nameof(type)}\" is not valid \"{nameof(IJsonRpcMethodHandler)}\" type");
        }

        Dictionary<string, MethodInfo> methodMetadata = [];
        foreach (var m in type.GetMethods())
        {
            var attr = (JsonRpcMethodAttribute?)
                m.GetCustomAttributes(typeof(JsonRpcMethodAttribute), true).FirstOrDefault();
            if (attr != null)
            {
                var name = GetJsonRpcMethodName(m, attr, opt);
                var key = JsonRpcConstants.DiKeyPrefix + name;

                services.AddKeyedScoped(typeof(IJsonRpcMethodHandler), key, type);
                methodMetadata.Add(name, m);
            }

            if (methodMetadata.Count > 0)
            {
                services.AddSingleton(new JsonRpcMethodMetadataOptions { Type = type, Methods = methodMetadata });
            }
        }

        return services;
    }

    /// <summary>
    /// Adds the <c>IJsonRpcMethodHandler </c> implementation to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcMethodHandler<T>(
        this IServiceCollection services,
        JsonRpcMethodOptions? opt = null
    )
        where T : IJsonRpcMethodHandler => services.AddJsonRpcMethodHandler(typeof(T), opt);

    /// <summary>
    /// Reads <c>IJsonRpcMethodHandler </c> implementations from the current app domain's
    /// assemblies and adds them to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcMethodsFromAssebly(
        this IServiceCollection services,
        JsonRpcMethodOptions? opt = null
    )
    {
        var handlerTypes = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(IsValidJsonRpcMethodHandlerType)
            .ToList();

        handlerTypes.ForEach(h => services.AddJsonRpcMethodHandler(h, opt));
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
    /// Set the <c>IJsonRpcExceptionHandler</c> implementation to the services.<br />
    /// <br/>
    /// NOTE:<br/>
    /// Using this method REPLACES the exception handler, since there can only be one!
    /// </summary>
    public static IServiceCollection SetJsonRpcExceptionHandler<T>(this IServiceCollection services)
        where T : IJsonRpcExceptionHandler =>
        services.Replace(ServiceDescriptor.Singleton(typeof(IJsonRpcExceptionHandler), typeof(T)));

    /// <summary>
    /// Maps the JSON RPC API to the WebSocket in the given route.
    /// </summary>
    public static WebApplication MapJsonRpcWebSocket(
        this WebApplication app,
        string route,
        bool shouldSendInitNotification = true
    )
    {
        app.Map(route, new JsonRpcWebSocketEndpointFactory(shouldSendInitNotification).Create());
        return app;
    }

    /// <summary>
    /// Maps the JSON RPC API schema endpoint to the given route.
    /// </summary>
    public static WebApplication MapJsonRpcSchema(this WebApplication app, string route)
    {
        app.MapGet(route, new JsonRpcSchemaEndpointFactory().Create());
        return app;
    }

    /// <summary>
    /// Adds JSON RPC WebSocket services to the services.
    /// </summary>
    private static IServiceCollection AddJsonRpcWebSocket(this IServiceCollection services) =>
        services
            .AddSingleton<IJsonRpcWebSocketProcessor, JsonRpcWebSocketProcessor>()
            .AddSingleton<IJsonRpcWebSocketContainer, JsonRpcWebSocketContainer>()
            .AddSingleton<IJsonRpcWebSocketIdGenerator, JsonRpcWebSocketIdGenerator>();

    private static bool IsValidJsonRpcMethodHandlerType(Type type) =>
        typeof(IJsonRpcMethodHandler).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract;

    private static string GetJsonRpcMethodName(
        MethodInfo method,
        JsonRpcMethodAttribute attr,
        JsonRpcMethodOptions? opt = null
    )
    {
        var original = method.Name;

        // Drop the async suffix from the method name, if one exists.
        const string asyncSuffix = "Async";
        if (original.EndsWith(asyncSuffix, StringComparison.OrdinalIgnoreCase))
        {
            original = original.Substring(0, original.Length - asyncSuffix.Length);
        }

        // 1. Name from the attribute
        if (!string.IsNullOrEmpty(attr.Name))
        {
            return attr.Name;
        }

        // 2. Name from the transformer
        if (opt?.NameTransformer != null)
        {
            return opt.NameTransformer(original);
        }

        // 3. Name with the naming policy
        if (opt?.NamingPolicy != null)
        {
            return opt.NamingPolicy.ConvertName(original);
        }

        // 4. Fallback to the original method name
        return original;
    }
}
