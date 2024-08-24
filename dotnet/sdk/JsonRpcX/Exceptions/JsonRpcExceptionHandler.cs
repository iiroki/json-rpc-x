namespace JsonRpcX.Exceptions;

public class JsonRpcExceptionHandler : IMessageExceptionHandler
{
    public Task<object?> HandleAsync(Exception ex, CancellationToken ct = default)
    {
        if (ex.GetType() == typeof(JsonRpcErrorException)) { }

        return Task.FromResult<object?>(null);
    }
}
