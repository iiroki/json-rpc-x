using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using JsonRpcX.Attributes;
using JsonRpcX.Extensions;
using JsonRpcX.Handlers;
using JsonRpcX.Models;
using JsonRpcX.Options;
using JsonRpcX.Services;
using JsonRpcX.Ws;

namespace JsonRpcX;

public static class DependencyExtensions
{
    /// <summary>
    /// Adds the base services from JSON RPC processing.
    /// </summary>
    public static IServiceCollection AddJsonRpc(this IServiceCollection services) =>
        services
            // Scoped (per request):
            .AddScoped<IJsonRpcContextManager, JsonRpcContextManager>()
            .AddScoped<IJsonRpcContextProvider>(sp => sp.GetRequiredService<IJsonRpcContextManager>())
            .AddScoped(sp => sp.GetRequiredService<IJsonRpcContextProvider>().Context)
            // Singleton:
            .AddSingleton<IJsonRpcInternalMethodFactory, JsonRpcInternalMethodFactory>()
            .AddSingleton<IJsonRpcInternalMethodContainer>(sp => sp.GetRequiredService<IJsonRpcInternalMethodFactory>())
            .AddSingleton<IMessageHandler<byte[], byte[]?>, JsonRpcMessageHandler>();

    /// <summary>
    /// Adds the <c>IJsonRpcMethodHandler </c> implementation to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcMethodHandler(
        this IServiceCollection services,
        Type type,
        JsonRpcOptions? opt = null
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
                services.AddSingleton(new JsonRpcInternalMethodOptions { Type = type, Methods = methodMetadata });
            }
        }

        return services;
    }

    /// <summary>
    /// Adds the <c>IJsonRpcMethodHandler </c> implementation to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcMethodHandler<T>(
        this IServiceCollection services,
        JsonRpcOptions? opt = null
    )
        where T : IJsonRpcMethodHandler => services.AddJsonRpcMethodHandler(typeof(T), opt);

    /// <summary>
    /// Reads <c>IJsonRpcMethodHandler </c> implementations from the current app domain's
    /// assemblies and adds them to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcMethodsFromAssebly(
        this IServiceCollection services,
        JsonRpcOptions? opt = null
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

    public static IServiceCollection AddJsonRpcWebSocket(this IServiceCollection services) =>
        services
            .AddSingleton<IWebSocketProcessor, WebSocketProcessor>()
            .AddSingleton<IWebSocketContainer, WebSocketContainer>();

    public static WebApplication MapJsonRpc(
        this WebApplication app,
        string path,
        bool shouldSendInitNotification = true
    )
    {
        app.Map(
            path,
            async (ctx) =>
            {
                if (ctx.WebSockets.IsWebSocketRequest)
                {
                    var container = app.Services.GetRequiredService<IWebSocketContainer>();
                    var processor = app.Services.GetRequiredService<IWebSocketProcessor>();
                    var jsonOptions = app.Services.GetService<JsonSerializerOptions>();

                    using var ws = await ctx.WebSockets.AcceptWebSocketAsync();

                    var task = processor.AttachAsync(ws, ctx);

                    // Send the initial notification
                    if (shouldSendInitNotification)
                    {
                        var notification = new JsonRpcRequest { Method = "init" };
                        var content = JsonSerializer.Serialize(notification, jsonOptions).GetUtf8Bytes();
                        await ws.SendAsync(content, WebSocketMessageType.Text, true, CancellationToken.None);
                    }

                    // Wait for the WebSocket processor to complete
                    await task;
                }
                else
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
        );

        return app;
    }

    public static WebApplication MapJsonRpcSchema(this WebApplication app, string path)
    {
        app.MapGet(
            path,
            async (ctx) =>
            {
                var container = app.Services.GetRequiredService<IJsonRpcInternalMethodContainer>();
                var jsonOptions = app.Services.GetService<JsonSerializerOptions>();

                // TODO: Think about what's needed in the schema?
                var schema = new { Methods = container.Methods.Keys.Order() };

                var bytes = JsonSerializer.Serialize(schema, jsonOptions).GetUtf8Bytes();
                await ctx.Response.BodyWriter.WriteAsync(bytes);
            }
        );

        return app;
    }

    private static bool IsValidJsonRpcMethodHandlerType(Type type) =>
        typeof(IJsonRpcMethodHandler).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract;

    private static string GetJsonRpcMethodName(
        MethodInfo method,
        JsonRpcMethodAttribute attr,
        JsonRpcOptions? opt = null
    )
    {
        var original = method.Name;

        // 1. Name from the attribute
        if (!string.IsNullOrEmpty(attr.Name))
        {
            return attr.Name;
        }

        // 2. Name from the transformer
        if (opt?.MethodNameTransformer != null)
        {
            return opt.MethodNameTransformer(original);
        }

        // 3. Name with the naming policy
        if (opt?.MethodNamingPolicy != null)
        {
            return opt.MethodNamingPolicy.ConvertName(original);
        }

        // 4. Fallback to the original method name
        return original;
    }
}
