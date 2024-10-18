using System.Reflection;
using JsonRpcX.Controllers;
using JsonRpcX.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Authorization;

internal static class JsonRpcAuthorizationResolver
{
    public static IEnumerable<IAuthorizeData> GetAuthorization(MethodInfo method)
    {
        var isAnonymous = GetAnonymousAttribute(method) != null;
        if (isAnonymous)
        {
            return []; // No authorization requirements
        }

        var methodAuth = method.GetCustomAttributes<AuthorizeAttribute>();
        if (method.DeclaringType == null || method.DeclaringType.GetInterface(nameof(IJsonRpcController)) == null)
        {
            throw new JsonRpcInitException($"Method's declaring type must implement '{nameof(IJsonRpcController)}'");
        }

        var controllerAuth = method.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>() ?? [];
        return methodAuth.Concat(controllerAuth);
    }

    private static AllowAnonymousAttribute? GetAnonymousAttribute(MethodInfo method) =>
        method.GetCustomAttribute<AllowAnonymousAttribute>()
        ?? method.DeclaringType?.GetCustomAttribute<AllowAnonymousAttribute>();
}
