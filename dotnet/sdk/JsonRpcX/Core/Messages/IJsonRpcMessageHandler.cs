namespace JsonRpcX.Core.Messages;

internal interface IJsonRpcMessageHandler<TIn, TOut>
{
    /// <summary>
    /// Handles the raw incoming message in the given context.
    /// </summary>
    Task<TOut> HandleAsync(TIn message, HttpContext httpCtx, CancellationToken ct = default);
}
