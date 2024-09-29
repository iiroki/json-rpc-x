using System.Collections.Concurrent;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Client;

internal class JsonRpcClientManager : IJsonRpcClientManager
{
    private readonly ConcurrentDictionary<string, IJsonRpcClient> _clients = [];

    //
    // Container
    //

    public IEnumerable<IJsonRpcClient> Clients => _clients.Values;

    public IEnumerable<IJsonRpcClient> Except(string? id) => Clients.Where(c => c.Id != id);

    public IEnumerable<IJsonRpcClient> Except(JsonRpcContext ctx) => Except(ctx.ClientId);

    //
    // Manager
    //

    public void Add(IJsonRpcClient client) => _clients.TryAdd(client.Id, client);

    public void Remove(string id) => _clients.TryRemove(id, out _);
}
