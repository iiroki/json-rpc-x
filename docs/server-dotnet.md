# JSON RPC X - Server (.NET)

This page contains the _JSON RPC X_ .NET server documentation.

## Using JSON RPC transports

TODO: HTTP transport not implemented.

## Register JSON RPC methods

JSON RPC methods can be registered collectively from the current app domain's assembly or separately one-by-one:

```cs
var builder = WebApplication.CreateBuilder(args);

// Add methods from the assembly...
builder.Services.AddJsonRpcMethodsFromAssebly();

// ...or add the method handlers one-by-one.
builder.Services.AddJsonRpcMethodHandler<JsonRpcExampleMethodHandler>();
```

### Configuring JSON RPC methods

JSON RPC methods can be further configured by using `JsonRpcMethodOptions` and
passing it as a parameter when registering methods:

```cs
var options = new JsonRpcMethodOptions
{
    MethodNamingPolicy = JsonNamingPolicy.CamelCase
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonRpcMethodsFromAssebly(options);
builder.Services.AddJsonRpcMethodHandler<JsonRpcExampleMethodHandler>(options);
```

## Dependency injection

_JSON RPC X_ is built on top of .NET's dependency injection, so the dependencies can be injected in the traditional way:

**`Program.cs`:**

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISingletonService, SingletonService>();
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddTransient<ITransientService, TransientService>();
```

**`JsonRpcExample.cs`**

```cs
public class JsonRpcExample(
    ISingletonService singletonService,
    IScopedService scopedService,
    ITransientService transientService
) : IJsonRpcMethodHandler
{
    private readonly ISingletonService _singletonService = singletonService;
    private readonly IScopedService _scopedService = scopedService;
    private readonly ITransientService _transientService = transientService;
}
```

**NOTES:**
- JSON RPC method handlers use service scopes, which enables using scoped services.

## Accessing JSON RPC context

JSON RPC context is served to JSON RPC method handlers via dependency injection
by injecting `JsonRpcContext`:

```cs
public class JsonRpcExample(
    JsonRpcContext ctx,
    ILogger<JsonRpcExample> logger
) : IJsonRpcMethodHandler
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod]
    public string? Method()
    {
        // Use the context in methods
        return _ctx.Request?.Id
    }
}
```

## Using custom JSON RPC middlewares

TODO: Feature not implemented.

## Configure JSON serialization

As _JSON RPC X_ uses `System.Text.Json`,
the JSON serialization options can be configured using `JsonSerializerOptions`:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(
    new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    }
);
```

**NOTES:**
- JSON-RPC 2.0 request and response property naming will always follow the specification,
  meaning that configuring `JsonSerializerOptions` wont affect them.

## Map JSON RPC schema endpoint

TODO
