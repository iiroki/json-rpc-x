using JsonRpcX.Attributes;
using JsonRpcX.Exceptions;
using JsonRpcX.Methods;

namespace JsonRpcX.Api.Methods;

public class JsonRpcUserMethods(ILogger<JsonRpcUserMethods> logger) : IJsonRpcMethodHandler
{
    private static readonly List<ExampleUser> Users = Enumerable
        .Range(1, 10)
        .Select(i => new ExampleUser(i, $"Example User {i}"))
        .ToList();

    private readonly ILogger _logger = logger;

    [JsonRpcMethod("getUsers")]
    public async Task<List<ExampleUser>> GetUsers(CancellationToken ct = default)
    {
        _logger.LogInformation("JSON RPC - Get users");
        await Task.Delay(500, ct);
        return Users;
    }

    [JsonRpcMethod("getUser")]
    public async Task<ExampleUser?> GetUser(long id, CancellationToken ct = default)
    {
        _logger.LogInformation("JSON RPC - Get user");
        await Task.Delay(500, ct);
        return Users.FirstOrDefault(u => u.Id == id);
    }

    [JsonRpcMethod]
    public async Task RegisterUser(ExampleUser user, CancellationToken ct = default)
    {
        _logger.LogInformation("JSON RPC - Register user: {U}", user);
        await Task.Delay(500, ct);
    }

    [JsonRpcMethod("AAAAAA")] // :D
    public void Dummy()
    {
        // NOP
    }

    [JsonRpcMethod]
    public void ThrowException()
    {
        throw new JsonRpcAuthException("No permission");
    }

    public record ExampleUser(long Id, string Name);

    public record GetUserParams(long Id);
}
