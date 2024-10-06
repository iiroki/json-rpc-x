using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Extensions;

public static class AuthorizationExtensions
{
    public static bool HasRoles(this IAuthorizeData auth) => !string.IsNullOrWhiteSpace(auth.Roles);

    public static List<string> GetRoles(this IAuthorizeData auth) =>
        auth.Roles?.Split(',').Select(r => r.Trim()).ToList() ?? throw new InvalidOperationException("No roles");
}
