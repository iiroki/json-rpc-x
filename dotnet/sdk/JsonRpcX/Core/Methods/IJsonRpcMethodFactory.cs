namespace JsonRpcX.Core.Methods;

/// <summary>
/// Factory for creating JSON RPC method invokers.
/// </summary>
internal interface IJsonRpcMethodFactory
{
    /// <summary>
    /// Creates a JSON RPC method invoker for the method.
    /// </summary>
    IJsonRpcMethodInvoker CreateInvocation(string method);
}
