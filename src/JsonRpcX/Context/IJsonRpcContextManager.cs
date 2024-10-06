using JsonRpcX.Domain.Models;

namespace JsonRpcX.Context;

internal interface IJsonRpcContextManager : IJsonRpcContextProvider
{
    void SetContext(JsonRpcContext ctx);
}
