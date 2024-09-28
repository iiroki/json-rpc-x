using System.Net;
using JsonRpcX.Helpers.Extensions;
using JsonRpcX.Transport.Http;
using JsonRpcX.Transport.Serialization;
using JsonRpcX.Transport.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Transport;

public static class DependencyExtensions
{
    //
    // Serialization
    //

    public static IServiceCollection AddJsonRpcSerializerDefaults(this IServiceCollection services) =>
        services
            .AddWithInterfaces<JsonRpcParser>(ServiceLifetime.Singleton)
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
        app.Map(
            route,
            async ctx =>
            {
                if (ctx.WebSockets.IsWebSocketRequest)
                {
                    var processor = ctx.RequestServices.GetRequiredService<IJsonRpcWebSocketProcessor>();
                    using var ws = await ctx.WebSockets.AcceptWebSocketAsync();
                    var task = processor.AttachAsync(ws, ctx);

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

    // <summary>
    /// Adds JSON RPC WebSocket services.
    /// </summary>
    public static IServiceCollection AddJsonRpcWebSocket(this IServiceCollection services) =>
        services.AddSingleton<IJsonRpcWebSocketProcessor, JsonRpcWebSocketProcessor>();
}
