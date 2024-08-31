namespace JsonRpcX;

public static class JsonRpcConstants
{
    public const string Version = "2.0";

    public const string DiKeyPrefix = "json-rpc@";

    /// <summary>
    /// JSON-RPC 2.0 pre-defined error codes.
    /// </summary>
    public enum ErrorCode
    {
        Unknown = 0,
        ParseError = -32700,
        InvalidRequest = -32600,
        MethodNotFound = -32601,
        InvalidParams = -32602,
        InternalError = -32603,
    }
}
