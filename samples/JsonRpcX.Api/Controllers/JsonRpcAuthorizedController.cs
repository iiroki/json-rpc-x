using JsonRpcX.Api.Authorization;
using JsonRpcX.Attributes;
using JsonRpcX.Controllers;
using JsonRpcX.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Api.Controllers;

[Authorize(Roles = "role1, role2")]
public class JsonRpcAuthorizedController(JsonRpcContext ctx, ILogger<JsonRpcAuthorizedController> logger)
    : IJsonRpcController
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod]
    public void Authorized()
    {
        var user = _ctx.User;
        _logger.LogInformation("Authorized: {U}", user.Identity?.Name);
    }

    [JsonRpcMethod]
    [Authorize(Policy = AuthorizationConstants.UsernamePolicy)]
    public void Policy()
    {
        var user = _ctx.User;
        _logger.LogInformation("Policy: {U}", user.Identity?.Name);
    }

    [JsonRpcMethod]
    [AllowAnonymous]
    public void Anonymous() => _logger.LogInformation("Anonymous");
}
