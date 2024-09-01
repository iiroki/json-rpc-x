using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

public class JsonRpcParseException(string? message = null)
    : JsonRpcErrorException(
        new JsonRpcError
        {
            Code = (int)JsonRpcConstants.ErrorCode.ParseError,
            Message = "Parse error",
            Data = !string.IsNullOrEmpty(message) ? new { Message = message } : null,
        }
    );
