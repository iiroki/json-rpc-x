using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.Serialization;

/// <summary>
/// JSON RPC message parser for a data type.<br />
/// <br />
/// JSON RPC message can be either a request or a response.
/// </summary>
public interface IJsonRpcMessageParser<T>
{
    /// <summary>
    /// Parses the raw chunk and converts it to a request or a response based on the chunk content.
    /// </summary>
    (JsonRpcRequest?, JsonRpcResponse?) Parse(T chunk);
}
