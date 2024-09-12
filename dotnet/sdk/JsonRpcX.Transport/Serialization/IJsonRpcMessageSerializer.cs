using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.Serialization;

public interface IJsonRpcMessageSerializer<T>
{
    /// <summary>
    /// Parses the raw chunk and converts it to a JSON RPC request or
    /// a JSON RPC response based on the chunk content.
    /// </summary>
    (JsonRpcRequest?, JsonRpcResponse?) Parse(T chunk);
}
