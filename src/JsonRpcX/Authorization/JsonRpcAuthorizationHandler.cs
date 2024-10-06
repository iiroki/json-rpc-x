using JsonRpcX.Domain.Models;
using JsonRpcX.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Authorization;

internal class JsonRpcAuthorizationHandler : IJsonRpcAuthorizationHandler
{
    public Task HandleAsync(JsonRpcContext ctx, IAuthorizeData data, CancellationToken ct = default)
    {
        if (ctx.User.Identity == null || !ctx.User.Identity.IsAuthenticated)
        {
            throw new NotImplementedException("TODO: Authorization identity error");
        }

        if (data.HasRoles())
        {
            foreach (var r in data.GetRoles())
            {
                if (ctx.User.IsInRole(r))
                {
                    return Task.CompletedTask;
                }
            }

            throw new NotImplementedException("TODO: Authorization role error");
        }

        return Task.CompletedTask;
    }
}
