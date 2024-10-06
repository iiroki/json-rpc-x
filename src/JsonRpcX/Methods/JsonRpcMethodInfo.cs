using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Methods;

internal class JsonRpcMethodInfo
{
    public required string Name { get; init; }

    public required MethodInfo Metadata { get; init; }

    public IAuthorizeData? Authorization { get; init; }
}
