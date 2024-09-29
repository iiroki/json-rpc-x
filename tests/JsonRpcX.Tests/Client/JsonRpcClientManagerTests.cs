using System.Security.Claims;
using JsonRpcX.Client;
using JsonRpcX.Domain.Models;

namespace JsonRpcX.Tests.Client;

public class JsonRpcClientManagerTests
{
    private readonly JsonRpcClientManager _manager = new();

    [Fact]
    public void Add_New_Added()
    {
        // Arrange
        var client = new TestJsonRpcClient();

        // Act
        _manager.Add(client);

        // Assert
        Assert.Equal([client], _manager.Clients);
    }

    [Fact]
    public void Add_DuplicateId_NotAdded()
    {
        // Arrange
        var client1 = new TestJsonRpcClient();
        var client2 = new TestJsonRpcClient(client1.Id, "ANOTHER_TRANSPORT");

        // Act
        _manager.Add(client1);
        _manager.Add(client2); // Same ID -> Should not be added

        // Assert
        Assert.Equal([client1], _manager.Clients);
    }

    [Fact]
    public void Remove_Existing_Removed()
    {
        // Arrange
        var client1 = new TestJsonRpcClient();
        var client2 = new TestJsonRpcClient(transport: "ANOTHER_TRANSPORT");
        _manager.Add(client1);
        _manager.Add(client2);

        // Act
        _manager.Remove(client1.Id);

        // Assert
        Assert.Equal([client2], _manager.Clients);
    }

    [Fact]
    public void Remove_Unknown_NotRemoved()
    {
        // Arrange
        var client1 = new TestJsonRpcClient();
        var client2 = new TestJsonRpcClient(transport: "ANOTHER_TRANSPORT");
        _manager.Add(client1);
        _manager.Add(client2);

        // Act
        _manager.Remove("unknown-id");

        // Assert
        Assert.Equal(2, _manager.Clients.Count());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Except_Existing_Filtered(bool useContext)
    {
        // Arrange
        var clients = Enumerable.Range(0, 100).Select(_ => new TestJsonRpcClient()).ToList();
        clients.ForEach(_manager.Add);

        var self = clients[clients.Count / 2];
        var ctx = new JsonRpcContext
        {
            Transport = "TEST",
            User = new ClaimsPrincipal(),
            ClientId = self.Id,
        };

        // Act
        var actual = useContext ? _manager.Except(ctx) : _manager.Except(ctx.ClientId);

        // Assert
        Assert.Equal(actual.Count(), clients.Count - 1);
        Assert.All(actual, c => Assert.NotEqual(self, c));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Except_Unknown_NotFiltered(bool useContext)
    {
        // Arrange
        var clients = Enumerable.Range(0, 100).Select(_ => new TestJsonRpcClient()).ToList();
        clients.ForEach(_manager.Add);

        var ctx = new JsonRpcContext
        {
            Transport = "TEST",
            User = new ClaimsPrincipal(),
            ClientId = Guid.NewGuid().ToString(),
        };

        // Act
        var actual = useContext ? _manager.Except(ctx) : _manager.Except(ctx.ClientId);

        // Assert
        Assert.Equal(actual.Count(), clients.Count);
    }

    private class TestJsonRpcClient(string? id = null, string? transport = null, ClaimsPrincipal? user = null)
        : IJsonRpcClient
    {
        public string Id { get; } = id ?? Guid.NewGuid().ToString();

        public string Transport { get; } = transport ?? "TEST";

        public ClaimsPrincipal User { get; } = user ?? new();

        public Task SendNotificationAsync(string method, object? @params, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task<JsonRpcResponse> SendRequestAsync(string method, object? @params, TimeSpan? timeout = null) =>
            throw new NotImplementedException();
    }
}
