using JsonRpcX.Domain.Models;

namespace JsonRpcX.Domain.Interfaces;

/// <summary>
/// JSON RPC processor that's responsible for parsing the request,
/// processing it and returning a serialized response.
/// </summary>
public interface IJsonRpcProcessor<TIn, TOut>
{
    /// <summary>
    /// 1. Sets the JSON RPC context for the request.<br />
    /// 2. Parses the JSON RPC request from the incoming message.<br />
    /// 3. Handles the request.<br />
    /// 4. IF an error occurred, handles the error.<br />
    /// 5. Serializes the response.
    /// </summary>
    Task<TOut?> ProcessAsync(TIn message, JsonRpcContext ctx, CancellationToken ct = default);
}
