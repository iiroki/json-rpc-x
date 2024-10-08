namespace JsonRpcX.Methods;

/// <summary>
/// Factory for creating JSON RPC method invokers.
/// </summary>
internal interface IJsonRpcMethodFactory
{
    /// <summary>
    /// Creates a JSON RPC method invoker for the method.
    /// </summary>
    IJsonRpcMethodInvoker Create(string method);
}
