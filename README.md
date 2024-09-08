# JSON RPC X

**_JSON RPC X_** is a JSON-RPC 2.0 .NET server with clients for .NET & TypeScript.

## Features

### General

- **JSON-RPC 2.0 support** (see ["Specification](#specification)).
- **Multiple transports:** HTTP & WebSockets.
    - Bidirectional communication over WebSockets!

### Server

- **Modern:** Built with .NET 8.
- **Lightweight:** Zero external dependencies!
- **Dependency injection capabilities:** Built on top of .NET's dependency injection.
- **Performant JSON serialization:** JSON serialization with `System.Text.Json`.
- **Middleware:** Enrich JSON RPC request pipelines with custom middleware.

### Client

TODO

## Installation

TODO

## Usage

This chapter only contains quickstarts for quickly setting up _JSON RPC X_.

See the full documentation for detailed information: **[DOCS](./docs/index.md)**

### Server

**Quickstart:**

1. Create a JSON RPC method handler class by tagging it with `IJsonRpcMethodHandler` interface:
    ```cs
    public class JsonRpcExampleMethodHandler(ILogger<JsonRpcExampleMethodHandler> logger) : IJsonRpcMethodHandler
    {
        private readonly ILogger _logger = logger;

        // ...
    }
    ```

2. Implement JSON RPC methods by marking the with `JsonRpcMethod` attribute:
    ```cs
    public class JsonRpcExampleMethodHandler(ILogger<JsonRpcExampleMethodHandler> logger) : IJsonRpcMethodHandler
    {
        private readonly ILogger _logger = logger;

        private static readonly List<string> Data =
        [
            "first",
            "second",
            "third"
        ];

        // By default, "Async" suffix is dropped from the method names.

        [JsonRpcMethod] // Method name -> "GetMany"
        public async Task<List<string>> GetManyAsync(CancellationToken ct)
        {
            _logger.LogInformation("Get many");
            await Task.Delay(100, ct)
            return Data;
        }

        [JsonRpcMethod] // Method name -> "Get"
        public async Task<string?> GetAsync(string id, CancellationToken ct)
        {
            _logger.LogInformation("Get: {Id}", id);
            await Task.Delay(100, ct)
            return Data.FirstOrDefault(d => d == id);
        }
    }
    ```

3. Register the method handler in `Program.cs` (the example uses HTTP transport):
    ```cs
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddJsonRpc();
    builder.Services.AddJsonRpcMethodsFromAssebly();
    builder.Services.AddJsonRpcWebSocket();

    var app = builder.Build();
    app.MapJsonRpcHttp("/json-rpc");

    await app.RunAsync();
    ```

4. Success!

## Specification

**_JSON RPC X_** uses and implements
[JSON-RPC 2.0](https://www.jsonrpc.org/specification)
specification.

Example JSON RPC payloads can be seen below.

### Request


_Example:_
```json
{
    "jsonrpc": "2.0",
    "method": "getUser",
    "id": "c890b461-fb55-4c31-a4d4-69ddaa7801d4",
    "params": {
        "id": 123
    }
}
```

### Response

_Example - Success:_
```json
{
    "jsonrpc": "2.0",
    "id": "c890b461-fb55-4c31-a4d4-69ddaa7801d4",
    "result": {
        "id": 123,
        "name": "Example User",
        "address": "Example Street 123",
        "isAdmin": false
    }
}
```

_Example - Error:_
```json
{
    "jsonrpc": "2.0",
    "id": "c890b461-fb55-4c31-a4d4-69ddaa7801d4",
    "error": {
        "code": 1,
        "message": "User not found",
        "data": {}
    }
}
```

### Notifications

_Example:_
```json
{
    "jsonrpc": "2.0",
    "method": "friendLoggedIn",
    "params": {
        "timestamp": "2024-08-21T08:07:00.000Z",
        "userId": 123
    }
}
```

## Motivation

The initial motivation for this project was learning more about the following topics:

- **Experimenting with new API communication protocols:**
  Previously I had mainly implemented APIs with REST, GraphQL and gRPC.
  I searched for existing API communication protocols as I figured that there must be some
  simple and lightweight protocol somewhere between the ones that I've already gotten familiar with.
  That's when I stumbled upon the JSON-RPC 2.0 and figured out that I wanted to build something
  on top of it for learning purposes.
- **Well-established specifications:**
  I wanted to develop something against a well-established specification,
  so in theory some external client libraries would also work against my server implementation.
  The JSON-RPC 2.0 specifaction seemed to tick that box,
  since there are lots of libraries built to support it.
- **Separation of communication and transport protocols:**
  In the past, I've mainly used one transport protocl per API communication protocol.
  Because of that, I wanted to build an API implementation that supports multiple transports
  for a single communication protocol.
  JSON-RPC 2.0 specification states that it's transport agnostic,
  which makes it fit the description.
- **Customizable library:**
  I wanted to experiment implementing a library, which would provide a clear and customizable
  interface for the library's end user.

## License

[**MIT**](./LICENSE)
