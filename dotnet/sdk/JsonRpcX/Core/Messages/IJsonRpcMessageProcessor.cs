using JsonRpcX.Models;

namespace JsonRpcX.Core.Messages;

internal interface IJsonRpcMessageProcessor<TIn, TOut>
{
    Task<TOut> ProcessAsync(TIn message, JsonRpcContext ctx, CancellationToken ct = default);
}
