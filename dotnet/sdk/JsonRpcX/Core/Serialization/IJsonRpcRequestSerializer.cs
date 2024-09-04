using JsonRpcX.Models;

namespace JsonRpcX.Core.Serialization;

public interface IJsonRpcRequestSerializer<T>
{
    /// <summary>
    /// Parses the raw chunk and converts it to a JSON RPC request.
    /// </summary>
    JsonRpcRequest? Parse(T chunk);
}
