namespace JsonRpcX.Handlers;

public interface IMessageHandler<TIn, TOut>
{
    Task<TOut> HandleAsync(TIn message, HttpContext httpCtx, CancellationToken ct = default);
}
