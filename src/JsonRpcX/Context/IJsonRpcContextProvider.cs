using JsonRpcX.Domain.Models;

namespace JsonRpcX.Context;

internal interface IJsonRpcContextProvider
{
    public JsonRpcContext Context { get; }
}
