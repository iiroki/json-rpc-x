using JsonRpcX.Domain.Constants;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Domain.Exceptions;

public class JsonRpcParseErrorException(string? message = null)
    : JsonRpcErrorException(
        new JsonRpcError
        {
            Code = (int)JsonRpcErrorCode.ParseError,
            Message = "Parse error",
            Data = !string.IsNullOrEmpty(message) ? new { Message = message } : null,
        }
    );
