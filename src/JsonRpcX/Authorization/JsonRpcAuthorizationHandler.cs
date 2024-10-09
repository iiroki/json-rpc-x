using System.Collections.Immutable;
using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;
using JsonRpcX.Methods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace JsonRpcX.Authorization;

internal class JsonRpcAuthorizationHandler : IJsonRpcAuthorizationHandler
{
    private readonly IAuthorizationService _authorization;
    private readonly ImmutableDictionary<string, List<IAuthorizationRequirement>> _requirements;

    public JsonRpcAuthorizationHandler(IAuthorizationService authorization, IJsonRpcMethodContainer methodContainer)
    {
        _authorization = authorization;

        // Store requirements for JSON RPC methods
        Dictionary<string, List<IAuthorizationRequirement>> requirements = [];
        foreach (var info in methodContainer.Methods.Values)
        {
            List<IAuthorizationRequirement> methodRequirements = [];

            if (!string.IsNullOrWhiteSpace(info.Authorization?.Roles))
            {
                var roles = info.Authorization.Roles.Split(',').Select(r => r.Trim());
                methodRequirements.Add(new RolesAuthorizationRequirement(roles));
            }

            if (methodRequirements.Count == 0 && info.Authorization != null)
            {
                methodRequirements.Add(new DenyAnonymousAuthorizationRequirement());
            }

            if (methodRequirements.Count > 0)
            {
                requirements.Add(info.Name, methodRequirements);
            }
        }

        _requirements = requirements.ToImmutableDictionary();
    }

    public async Task HandleAsync(JsonRpcContext ctx, CancellationToken _ = default)
    {
        if (ctx.Request == null)
        {
            throw new InvalidOperationException(
                $"Authorization handler should not be invoked when '{nameof(ctx.Request)}' == null"
            );
        }

        var method = ctx.Request.Method;
        if (!_requirements.TryGetValue(method, out var requirements))
        {
            return; // No authorization requirements
        }

        var res = await _authorization.AuthorizeAsync(ctx.User, null, requirements);
        if (res.Failure != null)
        {
            throw new JsonRpcAuthorizationExpection(res.Failure);
        }
    }
}
