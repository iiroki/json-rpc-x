namespace JsonRpcX.Exceptions;

public class JsonRpcMethodException(string method)
    : JsonRpcErrorException((int)JsonRpcConstants.ErrorCode.MethodNotFound, "Method not found", new { method });
