using JsonRpcX.Attributes;
using JsonRpcX.Controllers;
using JsonRpcX.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Api.Controllers;

[Authorize(Roles = "example")]
public class JsonRpcAuthorizedController(JsonRpcContext ctx, ILogger<JsonRpcAuthorizedController> logger)
    : IJsonRpcController
{
    private readonly JsonRpcContext _ctx = ctx;
    private readonly ILogger _logger = logger;

    [JsonRpcMethod]
    public void Save(IEnumerable<object> items, CancellationToken ct = default)
    {
        var user = _ctx.User;
        _logger.LogInformation("Items: {I}", items);
    }
}
