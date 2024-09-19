namespace JsonRpcX.Domain.Exceptions;

public class JsonRpcTimeoutException(string msg, TimeSpan timeout, Exception? cause = null)
    : JsonRpcException(msg, cause)
{
    public TimeSpan Timeout { get; } = timeout;
}
