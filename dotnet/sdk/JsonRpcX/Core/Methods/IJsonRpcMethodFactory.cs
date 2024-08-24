using JsonRpcX.Models;

namespace JsonRpcX.Core.Methods;

/// <summary>
/// Factory for creating internal JSON RPC method handlers.
/// </summary>
internal interface IJsonRpcMethodFactory : IJsonRpcMethodContainer
{
    /// <summary>
    /// Creates an internal JSON RPC method handler for the method in the given context.
    /// </summary>
    IJsonRpcMethodHandler2 CreateHandler(IServiceScope scope, string method, JsonRpcContext ctx);
}
