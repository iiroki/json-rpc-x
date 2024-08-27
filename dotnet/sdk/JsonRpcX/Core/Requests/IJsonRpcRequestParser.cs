using JsonRpcX.Models;

namespace JsonRpcX.Core.Requests;

/// <summary>
/// Parses JSON RPC requests.
/// </summary>
public interface IJsonRpcRequestParser<T>
{
    /// <summary>
    /// Parses the raw chunk and converts it to a JSON RPC request.
    /// </summary>
    JsonRpcRequest? Parse(T chunk);
}
