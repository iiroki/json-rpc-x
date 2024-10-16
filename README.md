# JSON RPC X

[![Build & Test](https://github.com/iiroki/json-rpc-x/actions/workflows/build.yml/badge.svg)](https://github.com/iiroki/json-rpc-x/actions/workflows/build.yml)

**_JSON RPC X_** is a simple and lightweight [JSON-RPC 2.0](https://www.jsonrpc.org/specification) .NET Server implementation.

_JSON RPC X_ leverages ASP.NET Core's built-in capabilities in order to
provide a flexible and easy-to-use developer experience.

## Features

- **JSON-RPC 2.0 implementation:**  
  Implemented according to JSON-RPC 2.0 specification,
  which means the server is compatible with existing JSON RPC clients.
  
- **Modern & lightweight:**  
  Built with .NET 8 and zero external dependencies!

- **Dependency injection capabilities:**  
  Utilizes .NET's well-established dependency injection system.

- **Performant JSON serialization:**  
  JSON serialization with `System.Text.Json`.

- **Customizable implementation:**  
  Enrich JSON RPC request pipelines with custom middleware and errors handling.

- **Multiple transports:**  
  Enforces the transport agnostic nature of JSON RPC by supporting multiple transports.
  Default transport support for HTTP & WebSocket,
  but the server can be extended with own custom transports!

- **Authorization:**  
  Support authorizing JSON RPC methods using .NET's built-in authorization system with `[Authorize]` attribute.

- **Truly bidirectional:**  
  Provides an easy way to access clients from transport connections
  with two-way communication capabilities.

## Installation

Install _JSON RPC X_ with .NET CLI:

```
dotnet add package JsonRpcX
```

### Projects

_JSON RPC X_ consists of multiple projects.

- `JsonRpcX`: The full _JSON RPC X_ library
- `JsonRpcX.Domain`: Common JSON RPC domain models
- `JsonRpcX.Client`: JSON RPC client components
- `JsonRpcX.Transport`: JSON RPC transport components
- `JsonRpcX.Helpers`: Common helpers for the other projects.

These projects can also be installed independently, if needed.

## Usage

_See the full documentation here: **[DOCS](https://github.com/iiroki/json-rpc-x/blob/main/docs/index.md)**_

**Quickstart:**

1. Create a JSON RPC controller class by tagging it with `IJsonRpcController` interface:
    ```cs
    public class JsonRpcExampleController(ILogger<JsonRpcExampleController> logger) : IJsonRpcController
    {
        private readonly ILogger _logger = logger;

        // ...
    }
    ```

2. Implement JSON RPC methods by marking them with `JsonRpcMethod` attribute:
    ```cs
    public class JsonRpcExampleController(ILogger<JsonRpcExampleController> logger) : IJsonRpcController
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

3. Register the controller in `Program.cs` (the example uses HTTP transport):
    ```cs
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddJsonRpc();
    builder.Services.AddJsonRpcControllersFromAssebly();

    var app = builder.Build();
    app.MapJsonRpcHttp("/json-rpc");

    await app.RunAsync();
    ```

4. Success!

## Motivation

The initial motivation for this project was learning more about the following topics:

- **Experimenting with new API communication protocols:**  
  Previously I had mainly implemented APIs with REST, GraphQL and gRPC.
  I searched for some simple and lightweight communication protocols
  which would also enable bidirectional communication.
  That's when I stumbled upon the JSON-RPC 2.0 specification.

- **Developing against well-established specifications:**  
  I wanted to develop something against a well-established specification,
  so in theory some external client libraries would also work against my server implementation.
  The JSON-RPC 2.0 specifaction seemed to also tick that box,
  since there are lots of client libraries around built to support it.

- **Separating communication and transport protocols:**  
  In the past, I've mainly used one transport protocol per API communication protocol.
  Because of that, I wanted to build an API implementation that supports multiple transports
  for a single communication protocol.
  JSON-RPC 2.0 specification states that it's transport agnostic,
  which makes it fit the description.

- **Implementing customizable libraries:**  
  I wanted to experiment implementing a library, which would provide a clear and customizable interface for the library's end user.

## Contribute

There probably won't be too much contributors,
so there's no predefined process for contributing to _JSON RPC X_.

If you wish to contribute to _JSON RPC X_,
open a GitHub issue about the change and we'll figure it out from there.

## License

[**MIT**](./LICENSE)
