using JsonRpcX.Models;

namespace JsonRpcX.Core.Serialization;

public interface IJsonRpcResponseSerializer<T>
{
    /// <summary>
    /// Serializes the JSON RPC response.<br />
    /// <br />
    /// If the returned value is null, the response should not be sent back to the client.
    /// </summary>
    T? Serialize(JsonRpcResponse? response);
}
