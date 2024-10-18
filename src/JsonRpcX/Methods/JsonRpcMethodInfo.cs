using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Methods;

/// <summary>
/// JSON RPC method's internal information for invoking the method.
/// </summary>
internal class JsonRpcMethodInfo
{
    /// <summary>
    /// Method name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Method's reflection metadata that can be used to invoke the method.
    /// </summary>
    public required MethodInfo Metadata { get; init; }

    /// <summary>
    /// Authorization data of the method.
    /// </summary>
    public IEnumerable<IAuthorizeData>? Authorization { get; init; }
}
