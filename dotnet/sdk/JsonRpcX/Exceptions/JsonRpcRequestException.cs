namespace JsonRpcX.Exceptions;

public class JsonRpcRequestException(string? message = null)
    : JsonRpcErrorException((int)JsonRpcConstants.ErrorCode.InvalidRequest, "Invalid Request", new { message });
