using System.Collections.Immutable;
using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;
using JsonRpcX.Methods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace JsonRpcX.Authorization;

internal class JsonRpcAuthorizationHandler : IJsonRpcAuthorizationHandler
{
    private readonly IAuthorizationService _service;
    private readonly ImmutableDictionary<string, JsonRpcMethodAuthorization> _methods;

    public JsonRpcAuthorizationHandler(IAuthorizationService service, IJsonRpcMethodContainer methodContainer)
    {
        _service = service;

        // Store authorization data for JSON RPC methods
        Dictionary<string, JsonRpcMethodAuthorization> builder = [];
        foreach (var method in methodContainer.Methods.Values)
        {
            if (method.Authorization != null)
            {
                var methodAuth = ToMethodAuthorization(method.Authorization);
                if (methodAuth.HasAny)
                {
                    builder.Add(method.Name, methodAuth);
                }
            }
        }

        _methods = builder.ToImmutableDictionary();
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
        if (!_methods.TryGetValue(method, out var authorization))
        {
            return; // No authorization requirements
        }

        List<Task<AuthorizationResult>> tasks = [];

        if (authorization.Roles.Count > 0)
        {
            tasks.Add(_service.AuthorizeAsync(ctx.User, null, authorization.Roles));
        }

        if (authorization.Policies.Count > 0)
        {
            tasks.AddRange(authorization.Policies.Select(p => _service.AuthorizeAsync(ctx.User, null, p)));
        }

        var results = await Task.WhenAll(tasks);
        var failure = results.Select(r => r.Failure).FirstOrDefault(f => f != null);
        if (failure != null)
        {
            throw new JsonRpcAuthorizationExpection(failure);
        }
    }

    private static JsonRpcMethodAuthorization ToMethodAuthorization(IEnumerable<IAuthorizeData> authorization)
    {
        var data = new JsonRpcMethodAuthorization { Roles = [], Policies = [] };

        foreach (var auth in authorization)
        {
            if (!string.IsNullOrWhiteSpace(auth.Roles))
            {
                var roles = auth.Roles.Split(',').Select(s => s.Trim()).ToList();
                data.Roles.Add(new RolesAuthorizationRequirement(roles));
            }

            if (!string.IsNullOrWhiteSpace(auth.Policy))
            {
                data.Policies.Add(auth.Policy);
            }
        }

        return data;
    }

    private readonly struct JsonRpcMethodAuthorization
    {
        public List<RolesAuthorizationRequirement> Roles { get; init; }

        public List<string> Policies { get; init; }

        public bool HasAny => Roles.Count > 0 || Policies.Count > 0;
    }
}
