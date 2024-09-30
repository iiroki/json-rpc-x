# JSON RPC X - Serialization

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

## Implement custom serialization

When requesting `IJsonRpcProcessor<TIn, TOut>` from the DI container,
the processor also requests `IJsonRpcParser<TIn>` and `IJsonRpcResponseSerializer<TOut>`.

By default, _JSON RPC X_ recognizes the following types:
- `byte[]`
- `string`
- `JsonElement`
- `JsonRpcRequest` (parser only) + `JsonRpcResponse`.

If you request `IJsonRpcProcessor<TIn, TOut>` for types unknown to _JSON RPC X_,
you have to implement and add serializers separately:

```cs
// JsonRpcCustomParser.cs
public class JsonRpcCustomParser: IJsonRpcParser<object>
{
    public (JsonRpcRequest?, JsonRpcResponse?) Parse(object chunk)
    {
        // ...
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IJsonRpcParser<object>, JsonRpcCustomParser>();

```
