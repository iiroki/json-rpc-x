using System.Security.Claims;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Client;

/// <summary>
/// JSON RPC client.
/// </summary>
public interface IJsonRpcClient
{
    /// <summary>
    /// Client ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Client transport type.
    /// </summary>
    string Transport { get; }

    /// <summary>
    /// User that's associated with the client.
    /// </summary>
    ClaimsPrincipal User { get; }

    /// <summary>
    /// Sends a request and awaits its response.
    /// </summary>
    Task<JsonRpcResponse> SendRequestAsync(string method, object? @params, TimeSpan? timeout = null);

    /// <summary>
    /// Sends a notification.
    /// </summary>
    Task SendNotificationAsync(string method, object? @params, CancellationToken ct = default);
}
