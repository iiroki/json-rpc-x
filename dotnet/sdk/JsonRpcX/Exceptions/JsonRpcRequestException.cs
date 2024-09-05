using JsonRpcX.Constants;

namespace JsonRpcX.Exceptions;

public class JsonRpcRequestException(string? message = null)
    : JsonRpcErrorException((int)JsonRpcErrorCode.InvalidRequest, "Invalid Request", new { message });
