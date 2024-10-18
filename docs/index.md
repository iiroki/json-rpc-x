# JSON RPC X - Documentation

Welcome to the _JSON RPC X_ .NET server documentation!

This is the main documentation page which contains the basic
information about the server.

Documentation about more advanced topics can be found under ["Advanced"](#advanced).

## Advanced

See these pages for more detailed documentation about more advanced topics:

- [**Authorization**](./authorization.md)
- [**Middleware**](./middleware.md)
- [**Errors**](./errors.md)
- [**Serialization**](./serialization.md)
- [**Bidirectional**](./bidirectional.md)
- [**Custom transports**](./custom-transports.md)

## Implement JSON RPC controllers

_JSON RPC X_ implements JSON RPC methods with "controllers".

JSON RPC controllers are implemented in two parts:
1. Create a class that extends `IJsonRpcController`
1. Create `public` methods and mark them with `JsonRpcMethod` attribute.

```cs
public class JsonRpcExampleController : IJsonRpcController
{
    // Gets the JSON RPC method name from the actual method -> "Hello"
    [JsonRpcMethod]
    public string Hello(string name) => $"Hello, {name}!";

    // By default, "async" suffix is dropped -> "DoWork"
    [JsonRpcMethod]
    public async Task<string> DoWorkAsync(CancellationToken ct = default)
    {
        const int workMs = 1000;
        await Task.Delay(workMs, ct);
        return $"Work done in {workMs} ms";
    }

    // The method name can also be overridden -> "increment"
    [JsonRpcMethod("increment")]
    public int OverrideName(int i) => i + 1;
}
```

**NOTES:**
- See ["Configure JSON RPC methods with options"](#configure-json-rpc-methods-with-options) below for top-level JSON RPC method configuration.
- JSON RPC methods support "async/await" programming model.
    - `CancellationToken` can be injected into the method by specifying it as **the last parameter**.
- JSON RPC method request and response schemas are automatically inferred from the method definition.
    - Under the hood, `System.Text.Json`'s `JsonElement.Deserialize` is used to parse the request params into the desired data types.

## Register JSON RPC methods

JSON RPC methods from the controllers can be registered collectively from the current app domain's assembly or separately one-by-one.

```cs
var builder = WebApplication.CreateBuilder(args);

// Add controllers from the assembly...
builder.Services.AddJsonRpcControllersFromAssebly();

// ...or add the controllers one-by-one.
builder.Services.AddJsonRpcController<JsonRpcExampleController>();
```

### Configure JSON RPC methods with options

JSON RPC methods can be further configured during registration
by using `JsonRpcMethodOptions` and passing it as a parameter.

```cs
var options = new JsonRpcMethodOptions
{
    NamingPolicy = JsonNamingPolicy.CamelCase
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonRpcControllersFromAssebly(options);
builder.Services.AddJsonRpcController<JsonRpcExampleController>(options);
```

## Access JSON RPC context

JSON RPC context is served to JSON RPC controllers via dependency injection
by injecting `JsonRpcContext`.

```cs
public class JsonRpcExampleController(
    JsonRpcContext ctx,
    ILogger<JsonRpcExampleController> logger
) : IJsonRpcController
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


## Use JSON RPC transports

_JSON RPC X_ provides multiple transports:
- **Default:** HTTP & WebSocket
- **Extensions:** _JSON RPC X_ can be extended by implementing custom transports.
    - (See ["Custom Transports"](./custom-transports.md))

### Use HTTP transport

```cs
var builder = WebApplication.CreateBuilder(args);

// No extra services needed
builder.Services.AddJsonRpc().AddJsonRpcControllersFromAssebly();

var app = builder.Build();
app.MapJsonRpcHttp("/json-rpc");

await app.RunAsync();
```

**NOTES:**
- _JSON RPC X_ implements HTTP transport as described here:
  ["JSON-RPC over HTTP"](https://www.jsonrpc.org/historical/json-rpc-over-http.html)
    - Only POST HTTP method is supported!

### Use WebSocket transport

```cs
var builder = WebApplication.CreateBuilder(args);

// Notice "AddJsonRpcWebSocket()"!
builder.Services.AddJsonRpc().AddJsonRpcControllersFromAssebly().AddJsonRpcWebSocket();

var app = builder.Build();
app.MapJsonRpcWebSocket("/json-rpc/ws");

await app.RunAsync();
```

## Dependency injection

_JSON RPC X_ is built on top of .NET's dependency injection, so the dependencies can be injected in the traditional way.

**`Program.cs`:**

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISingletonService, SingletonService>();
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddTransient<ITransientService, TransientService>();
```

**`JsonRpcExampleController.cs`**

```cs
public class JsonRpcExampleController(
    ISingletonService singletonService,
    IScopedService scopedService,
    ITransientService transientService
) : IJsonRpcController
{
    private readonly ISingletonService _singletonService = singletonService;
    private readonly IScopedService _scopedService = scopedService;
    private readonly ITransientService _transientService = transientService;
}
```

**NOTES:**
- JSON RPC controllers use service scopes, which enables using scoped services.
