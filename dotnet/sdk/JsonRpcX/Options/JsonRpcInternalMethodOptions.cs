using System.Reflection;

namespace JsonRpcX.Options;

/// <summary>
/// Internal options for initializing JSON RPC functionality.
/// </summary>
internal class JsonRpcInternalMethodOptions
{
    public required Type Type { get; init; }

    /// <summary>
    /// Method names and their invocation metadata.
    /// </summary>
    // TODO: Same method name in multiple JSON RPC APIs?
    public required Dictionary<string, MethodInfo> Methods { get; init; }
}
