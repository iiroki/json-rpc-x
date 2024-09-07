using JsonRpcX.Attributes;
using JsonRpcX.Domain.Models;
using JsonRpcX.Methods;

namespace JsonRpcX.Api.Methods;

public class JsonRpcUserMethods(JsonRpcContext ctx, ILogger<JsonRpcUserMethods> logger) : IJsonRpcMethodHandler
{
    private static readonly List<ExampleUser> Users = Enumerable
        .Range(1, 10)
        .Select(i => new ExampleUser(i, $"Example User {i}"))
        .ToList();

    private readonly JsonRpcContext _ctx = ctx;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod]
    public async Task<List<ExampleUser>> GetUsersAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("JSON RPC - Get users");
        await Task.Delay(500, ct);
        return Users;
    }

    [JsonRpcMethod]
    public async Task<ExampleUser?> GetUserAsync(long id, CancellationToken ct = default)
    {
        _logger.LogInformation("JSON RPC - Get user");
        await Task.Delay(500, ct);
        return Users.FirstOrDefault(u => u.Id == id);
    }

    [JsonRpcMethod]
    public async Task RegisterUserAsync(ExampleUser user, CancellationToken ct = default)
    {
        _logger.LogInformation("JSON RPC - Register user: {U}", user);
        await Task.Delay(500, ct);
    }

    [JsonRpcMethod]
    public string? Middleware() => _ctx.Data.TryGetValue("middleware", out var value) ? value.ToString() : null;

    [JsonRpcMethod]
    public void ThrowException() => throw new InvalidOperationException("Invalid method");

    [JsonRpcMethod("AAAAAA")] // :D
    public void Dummy()
    {
        // NOP
    }

    public record ExampleUser(long Id, string Name);

    public record GetUserParams(long Id);
}
