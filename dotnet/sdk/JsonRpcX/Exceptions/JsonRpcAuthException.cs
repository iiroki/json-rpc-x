namespace JsonRpcX.Exceptions;

/// <summary>
/// Exception for JSON RPC authorization errors.
/// </summary>
public class JsonRpcAuthException(string msg) : JsonRpcException(msg);
