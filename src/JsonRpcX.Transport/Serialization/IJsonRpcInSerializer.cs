using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.Serialization;

/// <summary>
/// JSON RPC serializer for incoming data types.<br />
/// <br />
/// JSON RPC chunk can be either a request or a response.
/// </summary>
public interface IJsonRpcInSerializer<TIn>
{
    /// <summary>
    /// Parses (deserializes) the raw chunk and converts it to a request or a response based on the content.
    /// </summary>
    (JsonRpcRequest?, JsonRpcResponse?) Parse(TIn chunk);
}
