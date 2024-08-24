namespace JsonRpcX.Exceptions;

public interface IMessageExceptionHandler
{
    Task<object?> HandleAsync(Exception ex, CancellationToken ct = default);
}
