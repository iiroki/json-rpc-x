using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Domain.Models;

/// <summary>
/// JSON RPC context.
/// </summary>
public class JsonRpcContext
{
    /// <summary>
    /// JSON RPC request currently being processed.<br />
    /// <br />
    /// If the value is null, the request could not be parsed.
    /// </summary>
    public JsonRpcRequest? Request { get; init; }

    public HttpContext? Http { get; init; }

    /// <summary>
    /// Context data, which can be used to pass simple data within the JSON RPC method pipeline.
    /// </summary>
    public Dictionary<string, object> Data { get; init; } = [];

    public JsonRpcContext WithRequest(JsonRpcRequest request)
    {
        if (Request != null)
        {
            throw new InvalidOperationException("JSON RPC context already has a request");
        }

        return new()
        {
            Request = request,
            Http = Http,
            Data = Data,
        };
    }
}
