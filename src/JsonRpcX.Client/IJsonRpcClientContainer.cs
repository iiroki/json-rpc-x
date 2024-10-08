using JsonRpcX.Domain.Models;

namespace JsonRpcX.Client;

/// <summary>
/// Container for JSON RPC clients that are connected to the server.
/// </summary>
public interface IJsonRpcClientContainer
{
    /// <summary>
    /// All JSON RPC clients.
    /// </summary>
    IEnumerable<IJsonRpcClient> Clients { get; }

    /// <summary>
    /// Returns all JSON RPC clients excepts the one with the given ID.<br />
    /// <br />
    /// This method is useful for getting all the other clients but the one that made the request.
    /// </summary>
    IEnumerable<IJsonRpcClient> Except(string? id);

    /// <inheritdoc cref="Except(string)"/>
    IEnumerable<IJsonRpcClient> Except(JsonRpcContext ctx);
}
