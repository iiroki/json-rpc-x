using System.Text.Json;
using JsonRpcX;
using JsonRpcX.Api.Authorization;
using JsonRpcX.Api.Middleware;
using JsonRpcX.Api.Services;
using JsonRpcX.Options;
using JsonRpcX.Transport;
using Microsoft.AspNetCore.Authentication;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
};

var jsonRpcOptions = new JsonRpcMethodOptions { NamingPolicy = JsonNamingPolicy.CamelCase };

//
// Builder
//

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddSingleton(jsonOptions)
    .AddHostedService<JsonRpcStatusWorker>()
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, ExampleAuthenticationHandler>(
        nameof(ExampleAuthenticationHandler),
        _ => { }
    );

// JSON RPC X methods:
builder
    .Services.AddJsonRpc()
    .AddJsonRpcControllersFromAssebly(jsonRpcOptions)
    .AddJsonRpcWebSocket()
    .AddJsonRpcMiddleware<JsonRpcExampleMiddleware>()
    .SetJsonRpcExceptionHandler<JsonRpcExampleExceptionHandler>();

//
// App
//

var app = builder.Build();
app.UseWebSockets();

app.MapJsonRpcSchema("/json-rpc");
app.MapJsonRpcHttp("/json-rpc");
app.MapJsonRpcWebSocket("/json-rpc/ws");

await app.RunAsync();
