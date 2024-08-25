using JsonRpcX.Models;

namespace JsonRpcX.Core.Methods;

/// <summary>
/// Factory for creating internal JSON RPC method handlers.
/// </summary>
internal interface IJsonRpcMethodFactory
{
    /// <summary>
    /// Creates an internal JSON RPC method handler for the method.
    /// </summary>
    IJsonRpcMethodInvocation CreateInvocation(string method);
}
