using JsonRpcX.Models;

namespace JsonRpcX.Core.Context;

internal interface IJsonRpcContextManager : IJsonRpcContextProvider
{
    void SetContext(JsonRpcContext ctx);
}
