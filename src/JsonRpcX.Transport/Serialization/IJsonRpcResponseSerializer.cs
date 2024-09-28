using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.Serialization;

/// <summary>
/// JSON RPC response serializer for a data type.
/// </summary>
public interface IJsonRpcResponseSerializer<T>
{
    /// <summary>
    /// Serializes the JSON RPC response.<br />
    /// <br />
    /// If the returned value is null, the response should not be sent back to the client.
    /// </summary>
    T? Serialize(JsonRpcResponse? response);
}
