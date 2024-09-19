using JsonRpcX.Domain.Models;

namespace JsonRpcX.Core.Context;

internal interface IJsonRpcContextManager : IJsonRpcContextProvider
{
    void SetContext(JsonRpcContext ctx);
}
