using JsonRpcX.Domain.Models;

namespace JsonRpcX.Requests;

/// <summary>
/// Handler for JSON RPC requests.
/// </summary>
internal interface IJsonRpcRequestHandler
{
    /// <summary>
    /// Handles the JSON RPC request in the given context.
    /// </summary>
    Task<JsonRpcResponse?> HandleAsync(JsonRpcRequest request, JsonRpcContext ctx, CancellationToken ct = default);
}
