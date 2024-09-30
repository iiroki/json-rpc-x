# JSON RPC X - Middleware

## Use JSON RPC middleware

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

**`JsonRpcExampleController.cs`:**
```cs
public class JsonRpcExampleController(JsonRpcContext ctx) : IJsonRpcController
{
    private readonly JsonRpcContext _ctx = ctx;

    [JsonRpcMethod]
    public string? Message() => _ctx.Data.GetValueOrDefault("message");
}
```

**NOTES:**
- As JSON RPC requests are processed within a service scope,
  the middleware's effects are available to the controllers.
