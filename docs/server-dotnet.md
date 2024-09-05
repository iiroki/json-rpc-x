# JSON RPC X - Server (.NET)

This page contains the _JSON RPC X_ .NET server documentation.

## Use JSON RPC transports

TODO: HTTP transport not implemented.

## Implement JSON RPC method handlers

_JSON RPC X_ implements JSON RPC methods with "method handlers".

JSON RPC method handlers are implemented in two parts:
1. Create a class that extends `IJsonRpcMethodHandler`
1. Create `public` methods and mark them with `JsonRpcMethod` attribute.

```cs
public class JsonRpcExampleMethodHandler : IJsonRpcMethodHandler
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

JSON RPC methods from the method handlers can be registered collectively from the current app domain's assembly or separately one-by-one.

```cs
var builder = WebApplication.CreateBuilder(args);

// Add methods from the assembly...
builder.Services.AddJsonRpcMethodsFromAssebly();

// ...or add the method handlers one-by-one.
builder.Services.AddJsonRpcMethodHandler<JsonRpcExampleMethodHandler>();
```

## Configure JSON RPC methods with options

JSON RPC methods can be further configured during registration
by using `JsonRpcMethodOptions` and passing it as a parameter.

```cs
var options = new JsonRpcMethodOptions
{
    NamingPolicy = JsonNamingPolicy.CamelCase
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonRpcMethodsFromAssebly(options);
builder.Services.AddJsonRpcMethodHandler<JsonRpcExampleMethodHandler>(options);
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

**`JsonRpcExampleMethodHandler.cs`**

```cs
public class JsonRpcExampleMethodHandler(
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

## Access JSON RPC context

JSON RPC context is served to JSON RPC method handlers via dependency injection
by injecting `JsonRpcContext`.

```cs
public class JsonRpcExampleMethodHandler(
    JsonRpcContext ctx,
    ILogger<JsonRpcExampleMethodHandler> logger
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

## Use custom JSON RPC middleware

_JSON RPC X_ supports implementing custom middleware and
running them as part of the JSON RPC method pipeline.

Custom JSON RPC middleware can be implemented by implementing `IJsonRpcMiddleware` interface
and adding the middleware to the services.

**`JsonRpcExampleMiddleware.cs`:**

```cs
public class JsonRpcExampleMiddleware(JsonRpcContext ctx) : IJsonRpcMiddleware
{
    private readonly JsonRpcContext _ctx = ctx;

    public Task HandleAsync(CancellationToken ct = default)
    {
        _ctx.Data.Add("message", "Middleware was here!");
        return Task.CompletedTask;
    }
}
```

**`Program.cs`:**

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonRpcMiddleware<JsonRpcExampleMiddleware>();
```

**`JsonRpcExampleMethodHandler.cs`:**
```cs
public class JsonRpcExampleMethodHandler(JsonRpcContext ctx) : IJsonRpcMethodHandler
{
    private readonly JsonRpcContext _ctx = ctx;

    [JsonRpcMethod]
    public string? Message() => _ctx.Data.GetValueOrDefault("message");
}
```

**NOTES:**
- As JSON RPC requests are processed within a service scope,
  the middleware's effects are available to the method handlers.

## Handle JSON RPC errors

_JSON RPX X_ handles the following errors according to the JSON-RPC 2.0 specification:

- "General error" -> `JsonRpcErrorException`
    - The exception includes a JSON RPC error
    - All the errors below extend from this
    - This class can be extended to define custom JSON RPC error exception
- -32700 = "Parse error" -> `JsonRpcParseException`
- -32600 = "Invalid Request" -> `JsonRpcRequestException`
- -32601 = "Method not found" -> `JsonRpcMethodException`
- -32602 = "Invalid params" -> `JsonRpcParamException`
- -32603 = "Internal error" -> All unknown and unhandled errors.

_JSON RPC X_'s error handling work in the following order:
1. **Custom:** If a custom error handler exists, it's invoked first.
    - If the custom error handler did not exist or it did not produce a JSON RPC error,
      the default error handler is invoked.
1. **Default:** The default error handler checks if the thrown exception is `JsonRpcErrorException`...
    - If yes: the exception's JSON RPC error is returned.
    - If no: an internal error is created from the thrown exception.
1. **Response:** JSON RPC response is created from the JSON RPC error.

See below for custom JSON RPC error handler documentation.

### Custom JSON RPC error handler

If the default error handling of _JSON RPC X_ is not enough,
one might implement a custom JSON RPC error handler.

The custom error handler can be implemented with `IJsonRpcExceptionHandler`.

**`JsonRpcCustomExceptionHandler.cs`:**
```cs
public class JsonRpcCustomExceptionHandler(IExampleService service) : IJsonRpcExceptionHandler
{
    // Example service from the DI container
    private readonly IExampleService _service = service;

    public Task<JsonRpcError?> HandleAsync(Exception ex, CancellationToken ct = default)
    {
        // Create a custom JSON RPC error from the exception
        return Task.FromResult<JsonRpcError?>(null);
    }
}
```

The custom error handler can be registered with `SetJsonRpcExceptionHandler` extension method.

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.SetJsonRpcExceptionHandler<JsonRpcCustomExceptionHandler>();
```

**NOTES:**
- The custom error handler is invoked BEFORE the default one,
  so the custom one takes priority over the default one.
- The error handler has a scoped service lifetime,
  which means that all services from the DI container can be used in the custom handler.
- By returning `null`, the default error handler is invoked.
  This can be utilized implement some custom functionality for errors
  without altering the JSON RPC response.

## Configure JSON serialization

As _JSON RPC X_ uses `System.Text.Json`,
the JSON serialization options can be configured using `JsonSerializerOptions`.

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
