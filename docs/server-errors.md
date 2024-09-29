# JSON RPC X - Server: Errors

## JSON RPC error types

_JSON RPC X_ supports all the errors defined in the JSON-RPC 2.0 specification
and has C# exception counterparts of them.

All the JSON RPC error exceptions extend from `JsonRpcErrorException`,
which is an exception class that contains a JSON RPC error.

_JSON RPC X_ supports the following built-in JSON RPC error types:

| **JSON RPC code** | **Description** | **C# exception** |
| :-----: | ----- | ----- |
| -32700 | "Parse error" | `JsonRpcParseException` |
| -32600 | "Invalid Request" | `JsonRpcRequestException` |
| -32601 | "Method not found" | `JsonRpcMethodException` |
| -32602 | "Invalid params" | `JsonRpcParamException` |
| -32603 | "Internal error" | All unhandled errors |
| (any) | Custom error | `JsonRpcErrorException` |

By throwing any of the exceptions listed above,
the default error handler implementation will catch the exception
and create an error response from it.

**Example:**
```cs
throw new JsonRpcErrorException(12345, "Custom error message");
```

### Custom JSON RPC error types

Custom JSON RPC error types can be implemented by extending from `JsonRpcErrorException`.

**`JsonRpcExampleException.cs`:**
```cs
public class JsonRpcExampleException()
    : JsonRpcErrorException(new JsonRpcError { Code = 123, Message = "Example message" });
```

**`JsonRpcExampleMethodHandler.cs`:**
```cs
public class JsonRpcExampleMethodHandler : IJsonRpcMethodHandler
{
    [JsonRpcMethod]
    public void Throw() => throw new JsonRpcExampleException();
}
```

**NOTE:** As the custom exception extends `JsonRpcErrorException`,
it will be handled by the default error handler.

## JSON RPC error handling

_JSON RPC X_ handles the errors that occur during JSON RPC request processing with
the default error handler, which can be overridden with a custom error handler.

_JSON RPC X_'s error handling works in the following order:

1. **Custom:** If a custom error handler exists, it's invoked first.
    - If the custom error handler did not exist or it did not produce a JSON RPC error,
      the default error handler is invoked.
    - See ["Custom JSON RPC error handler"](#custom-json-rpc-error-handler)
      below for implementation instructions.
1. **Default:** The default error handler checks if the thrown exception is `JsonRpcErrorException`...
    - **Yes:** The exception's JSON RPC error is returned.
    - **No:** An internal error is created from the unhandled exception.
        - Unhandled exceptions are also logged as errors.
1. **Response:** JSON RPC response is created from the JSON RPC error.
 
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
  without altering the default JSON RPC response.
