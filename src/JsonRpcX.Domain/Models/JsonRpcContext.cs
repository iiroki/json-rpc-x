using System.Security.Claims;

namespace JsonRpcX.Domain.Models;

/// <summary>
/// JSON RPC context.
/// </summary>
public class JsonRpcContext
{
    /// <summary>
    /// JSON RPC transport type.
    /// </summary>
    public required string Transport { get; init; }

    /// <summary>
    /// JSON RPC request currently being processed.<br />
    /// <br />
    /// If the value is null, the request could not be parsed.
    /// </summary>
    public JsonRpcRequest? Request { get; init; }

    /// <summary>
    /// User that made the request.
    /// </summary>
    public required ClaimsPrincipal User { get; init; }

    /// <summary>
    /// JSON RPC client ID, if one is associated with the request.
    /// </summary>
    public string? ClientId { get; init; }

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
            Transport = Transport,
            User = User,
            ClientId = ClientId,
            Data = Data,

            Request = request,
        };
    }
}
