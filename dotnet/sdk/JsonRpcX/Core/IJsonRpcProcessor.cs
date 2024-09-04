using JsonRpcX.Models;

namespace JsonRpcX.Core;

internal interface IJsonRpcProcessor<TIn, TOut>
{
    Task<TOut?> ProcessAsync(TIn message, JsonRpcContext ctx, CancellationToken ct = default);
}
