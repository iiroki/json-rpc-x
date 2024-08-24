namespace JsonRpcX.Models;

/// <summary>
/// JSON RPC context.
/// </summary>
public class JsonRpcContext
{
    /// <summary>
    /// JSON RPC request currently being processed.
    /// </summary>
    public required JsonRpcRequest Request { get; init; }

    // TODO: Is this needed for all transports?
    public required HttpContext Http { get; init; }

    /// <summary>
    /// JSON RPC request ID.
    /// </summary>
    public string? RequestId => Request.Id;
}
