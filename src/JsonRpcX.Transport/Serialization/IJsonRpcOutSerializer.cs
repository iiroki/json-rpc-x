using JsonRpcX.Domain.Models;

namespace JsonRpcX.Transport.Serialization;

/// <summary>
/// JSON RPC serializer for outgoing data types.
/// </summary>
public interface IJsonRpcOutSerializer<T>
{
    /// <summary>
    /// Serializes the JSON RPC response.<br />
    /// <br />
    /// If the returned value is null, the response should not be sent back to the client.
    /// </summary>
    T? Serialize(JsonRpcResponse? response);
}
