using JsonRpcX.Models;

namespace JsonRpcX.Core.Requests;

/// <summary>
/// Handles JSON RPC requests.
/// </summary>
internal interface IJsonRpcRequestHandler
{
    /// <summary>
    /// Handles the JSON RPC request in the given context.
    /// </summary>
    Task<JsonRpcResponse?> HandleAsync(JsonRpcRequest request, JsonRpcContext ctx, CancellationToken ct = default);
}
