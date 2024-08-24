using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

public class JsonRpcErrorException : JsonRpcException
{
    public JsonRpcError Error { get; }

    public JsonRpcErrorException(JsonRpcContext? ctx, JsonRpcError error)
        : base(ctx, error.Message)
    {
        Error = error;
    }

    public JsonRpcErrorException(JsonRpcContext? ctx, int code, string message, object? data = null)
        : base(ctx, message)
    {
        Error = new JsonRpcError
        {
            Code = code,
            Message = message,
            Data = data,
        };
    }

    public JsonRpcErrorException(JsonRpcContext? ctx, int code, string message)
        : base(ctx, message)
    {
        Error = new JsonRpcError { Code = code, Message = message };
    }
}
