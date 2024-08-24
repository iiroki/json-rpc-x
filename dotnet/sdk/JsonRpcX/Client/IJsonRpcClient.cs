using JsonRpcX.Models;

namespace JsonRpcX.Client;

// TODO: This is not yet supported/implemented,
// but is required for bidirectional communication!
public interface IJsonRpcClient
{
    event EventHandler<JsonRpcResponse> NotificationReceived;

    Task<JsonRpcResponse> SendRequest();

    Task SendNotification();
}
