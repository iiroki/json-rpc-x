using JsonRpcX.Helpers.Extensions;
using JsonRpcX.Transport.Http;
using JsonRpcX.Transport.Serialization;
using JsonRpcX.Transport.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport;

public static class DependencyExtensions
{
    //
    // Serialization
    //

    public static IServiceCollection AddJsonRpcSerializerDefaults(this IServiceCollection services) =>
        services
            .AddWithInterfaces<JsonRpcMessageParser>(ServiceLifetime.Singleton)
            .AddWithInterfaces<JsonRpcResponseSerializer>(ServiceLifetime.Singleton);

    //
    // HTTP
    //

    /// <summary>
    /// Maps the JSON RPC API to the HTTP POST in the given route.
    /// </summary>
    /// <remarks>
    /// See: <see href="https://www.jsonrpc.org/historical/json-rpc-over-http.html">JSON-RPC over HTTP</see>
    /// </remarks>
    public static WebApplication MapJsonRpcHttp(this WebApplication app, string route)
    {
        var transport = new JsonRpcHttpTransport();
        app.MapPost(route, transport.Delegate);
        return app;
    }

    //
    // WebSockets
    //

    /// <summary>
    /// Maps the JSON RPC API to the WebSocket in the given route.
    /// </summary>
    public static WebApplication MapJsonRpcWebSocket(this WebApplication app, string route)
    {
        var transport = new JsonRpcWebSocketTransport();
        app.Map(route, transport.Delegate);
        return app;
    }

    // <summary>
    /// Adds JSON RPC WebSocket services.
    /// </summary>
    public static IServiceCollection AddJsonRpcWebSocket(this IServiceCollection services) =>
        services.AddSingleton<IJsonRpcWebSocketProcessor, JsonRpcWebSocketProcessor>();
}
