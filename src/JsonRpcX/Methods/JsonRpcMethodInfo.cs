using System.Reflection;

namespace JsonRpcX.Methods;

internal class JsonRpcMethodInfo
{
    public required string Name { get; init; }

    public required MethodInfo Metadata { get; init; }

    public bool? IsAuthorized { get; init; }

    public IEnumerable<string>? Roles { get; init; }
}
