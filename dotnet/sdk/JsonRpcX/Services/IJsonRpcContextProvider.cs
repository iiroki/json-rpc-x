using JsonRpcX.Models;

namespace JsonRpcX.Services;

public interface IJsonRpcContextProvider
{
    public JsonRpcContext Context { get; }
}
