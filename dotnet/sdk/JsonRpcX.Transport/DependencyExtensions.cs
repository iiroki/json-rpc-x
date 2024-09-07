using JsonRpcX.Transport.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport;

public static class DependencyExtensions
{
    //
    // HTTP
    //

    // TODO...

    //
    // WebSockets
    //

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

    // <summary>
    /// Adds JSON RPC WebSocket services to the services.
    /// </summary>
    public static IServiceCollection AddJsonRpcWebSocket(this IServiceCollection services) =>
        services
            .AddSingleton<IJsonRpcWebSocketProcessor, JsonRpcWebSocketProcessor>()
            .AddSingleton<IJsonRpcWebSocketContainer, JsonRpcWebSocketContainer>()
            .AddSingleton<IJsonRpcWebSocketIdGenerator, JsonRpcWebSocketIdGenerator>();
}
