using System.Text.Json;
using JsonRpcX;
using JsonRpcX.Api.Services;
using JsonRpcX.Options;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
};

var jsonRpcOptions = new JsonRpcMethodOptions { MethodNamingPolicy = JsonNamingPolicy.CamelCase };

//
// Builder
//

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(jsonOptions);
builder.Services.AddHostedService<JsonRpcStatusWorker>();

// JSON RPC X methods:
builder.Services.AddJsonRpc();
builder.Services.AddJsonRpcWebSocket(); // Requires "app.UseWebSockets()"
builder.Services.AddJsonRpcMethodsFromAssebly(jsonRpcOptions);

//
// App
//

var app = builder.Build();
app.UseWebSockets();
app.MapJsonRpcWebSocket("/ws");
app.MapJsonRpcSchema("/");

await app.RunAsync();
