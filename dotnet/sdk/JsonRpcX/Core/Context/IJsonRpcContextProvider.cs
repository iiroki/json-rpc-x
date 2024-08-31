using JsonRpcX.Models;

namespace JsonRpcX.Core.Context;

internal interface IJsonRpcContextProvider
{
    public JsonRpcContext Context { get; }
}
