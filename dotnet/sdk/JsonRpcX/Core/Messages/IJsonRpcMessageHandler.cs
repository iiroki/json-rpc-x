using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal interface IJsonRpcMessageHandler<TIn>
{
    /// <summary>
    /// Parses the raw message and converts it to a JSON RPC request.
    /// </summary>
    JsonRpcRequest? Parse(TIn message);

    /// <summary>
    /// Handles the JSON RPC request in the given context.
    /// </summary>
    Task<JsonRpcResponse?> HandleAsync(JsonRpcRequest request, JsonRpcContext ctx, CancellationToken ct = default);
}
