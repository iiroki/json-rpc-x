using JsonRpcX.Domain.Constants;

namespace JsonRpcX.Domain.Exceptions;

public class JsonRpcInvalidRequestException(string? message = null)
    : JsonRpcErrorException((int)JsonRpcErrorCode.InvalidRequest, "Invalid Request", new { message });
