using JsonRpcX.Models;

namespace JsonRpcX.Services;

internal class JsonRpcContextManager : IJsonRpcContextManager
{
    private JsonRpcContext? _ctx;

    public JsonRpcContext Context => _ctx ?? throw new InvalidOperationException("JSON RPC context not available");

    public void SetContext(JsonRpcContext ctx)
    {
        _ctx = ctx;
    }
}
