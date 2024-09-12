using JsonRpcX.Domain.Constants;

namespace JsonRpcX.Domain.Exceptions;

public class JsonRpcMethodException(string method)
    : JsonRpcErrorException((int)JsonRpcErrorCode.MethodNotFound, "Method not found", new { method });
