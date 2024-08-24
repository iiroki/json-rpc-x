using JsonRpcX.Handlers;
using JsonRpcX.Models;

namespace JsonRpcX.Services;

/// <summary>
/// Factory for creating internal JSON RPC method handlers.
/// </summary>
internal interface IJsonRpcInternalMethodFactory : IJsonRpcInternalMethodContainer
{
    /// <summary>
    /// Creates an internal JSON RPC method handler for the method in the given context.
    /// </summary>
    IJsonRpcInternalMethodHandler CreateHandler(IServiceScope scope, string method, JsonRpcContext ctx);
}
