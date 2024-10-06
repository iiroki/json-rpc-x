using JsonRpcX.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Authorization;

public interface IJsonRpcAuthorizationHandler
{
    Task HandleAsync(JsonRpcContext ctx, IAuthorizeData data, CancellationToken ct = default);
}
