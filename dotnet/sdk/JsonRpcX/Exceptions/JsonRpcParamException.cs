using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

public class JsonRpcParamException(string? detail = null)
    : JsonRpcErrorException(
        new JsonRpcError
        {
            Code = (int)JsonRpcConstants.ErrorCode.InvalidParams,
            Message = "Invalid params",
            Data = !string.IsNullOrEmpty(detail) ? new { Detail = detail } : null,
        }
    );
