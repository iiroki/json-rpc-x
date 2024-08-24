using System.Collections.Immutable;
using System.Reflection;

namespace JsonRpcX.Handlers;

/// <summary>
/// Internal interface for coupling JSON RPC method handlers and their method invocation metadata.
/// </summary>
internal interface IJsonRpcInternalMethodHandler
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
