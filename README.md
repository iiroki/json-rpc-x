# JSON RPC X

**_JSON RPC X_** is a modern and developer-friendly
[JSON-RPC 2.0](https://www.jsonrpc.org/specification) .NET server library for ASP.NET Core.

## Features

- **JSON-RPC 2.0:**  
  Implementation according to JSON-RPC 2.0 specification,
  which means the server is compatible with existing JSON RPC clients.
  
- **Modern:**  
  Built with .NET 8.

- **Lightweight:**  
  Zero external dependencies!

- **Dependency injection capabilities:**  
  Built on top of .NET's dependency injection.

- **Performant JSON serialization:**  
  JSON serialization with `System.Text.Json`.

- **Middleware:**  
  Enrich JSON RPC request pipelines with custom middleware.

- **Multiple transports:**  
  Enforces the transport agnostic nature of JSON RPC by supporting multiple transports.
  Default transport support for HTTP & WebSocket,
  but the server can be extended with own custom transports!

- **Truly bidirectional:**  
  Provides an easy way to access clients from transport connections
  with two-way communication capabilities.

## Installation

TODO

## Usage

_See the full .NET server documentation here: **[DOCS](./docs/server.md)**_

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

    var app = builder.Build();
    app.MapJsonRpcHttp("/json-rpc");

    await app.RunAsync();
    ```

4. Success!

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
