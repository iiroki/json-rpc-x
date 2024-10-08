using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Domain.Exceptions;

public class JsonRpcInvalidParamsException(string? message = null)
    : JsonRpcErrorException(
        new JsonRpcError
        {
            Code = (int)JsonRpcErrorCode.InvalidParams,
            Message = "Invalid params",
            Data = !string.IsNullOrEmpty(message) ? new { Message = message } : null,
        }
    );
