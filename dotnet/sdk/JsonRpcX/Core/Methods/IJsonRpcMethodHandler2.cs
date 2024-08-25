using System.Reflection;
using JsonRpcX.Methods;

namespace JsonRpcX.Core.Methods;

/// <summary>
/// Internal interface for coupling JSON RPC method handlers and their method invocation metadata.
/// </summary>
// TODO: A better name :D
internal interface IJsonRpcMethodInvocation
{
    /// <summary>
    /// JSON RPC method handler to invoke the methods against.
    /// </summary>
    public IJsonRpcMethodHandler Handler { get; }

    /// <summary>
    /// JSON RPC method invocation metadata.
    /// </summary>
    public MethodInfo Method { get; }
}
