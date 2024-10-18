namespace JsonRpcX.Domain.Exceptions;

/// <summary>
/// Error that occurs during JSON RPC initialization.
/// </summary>
public class JsonRpcInitException(string msg, Exception? cause = null) : JsonRpcException(msg, cause);
