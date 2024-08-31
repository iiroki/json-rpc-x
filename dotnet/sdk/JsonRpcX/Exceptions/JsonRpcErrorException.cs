using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

public class JsonRpcErrorException : JsonRpcException
{
    public JsonRpcError Error { get; }

    public JsonRpcErrorException(JsonRpcError error)
        : base(error.Message)
    {
        Error = error;
    }

    public JsonRpcErrorException(int code, string message, object? data = null)
        : base(message)
    {
        Error = new JsonRpcError
        {
            Code = code,
            Message = message,
            Data = data,
        };
    }

    public JsonRpcErrorException(int code, string message)
        : base(message)
    {
        Error = new JsonRpcError { Code = code, Message = message };
    }
}
