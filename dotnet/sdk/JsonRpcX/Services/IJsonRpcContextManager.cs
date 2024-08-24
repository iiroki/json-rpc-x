using JsonRpcX.Models;

namespace JsonRpcX.Services;

internal interface IJsonRpcContextManager : IJsonRpcContextProvider
{
    void SetContext(JsonRpcContext ctx);
}
