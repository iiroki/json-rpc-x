using JsonRpcX.Models;

namespace JsonRpcX.Exceptions;

public class JsonRpcException(JsonRpcContext? ctx, string msg) : Exception(msg)
{
    public JsonRpcContext? Context = ctx;
}
