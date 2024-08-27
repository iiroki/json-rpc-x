using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal interface IJsonRpcMessageHandler<TIn>
{
    /// <summary>
    /// Handles the JSON RPC request in the given context.
    /// </summary>
    Task<JsonRpcResponse?> HandleAsync(JsonRpcRequest request, JsonRpcContext ctx, CancellationToken ct = default);
}
