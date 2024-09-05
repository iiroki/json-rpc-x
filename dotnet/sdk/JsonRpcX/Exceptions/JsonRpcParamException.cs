using JsonRpcX.Constants;
using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

public class JsonRpcParamException(string? message = null)
    : JsonRpcErrorException(
        new JsonRpcError
        {
            Code = (int)JsonRpcErrorCode.InvalidParams,
            Message = "Invalid params",
            Data = !string.IsNullOrEmpty(message) ? new { Message = message } : null,
        }
    );
