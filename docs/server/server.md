# JSON RPC X - Server

This page contains the _JSON RPC X_ .NET server documentation.

## Register JSON RPC methods

JSON RPC methods can be registered collectively from the current app domain's assembly or separately one-by-one:

```cs
// Use "JsonRpcMethodOptions" to configure registered methods
var options = new JsonRpcMethodOptions
{
    MethodNamingPolicy = JsonNamingPolicy.CamelCase
};

var builder = WebApplication.CreateBuilder(args);

// Default options will be used if "options" can be omitted!

// 1. Add methods from the assembly (with options)
builder.Services.AddJsonRpcMethodsFromAssebly(options);

// 2. Add method handlers separately (without options = default options)
builder.Services.AddJsonRpcMethodHandler<JsonRpcExampleMethodHandler>();
```

## Configure JSON options

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
