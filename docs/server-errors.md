# JSON RPC X - Server: Errors

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

**`Program.cs`:**

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
