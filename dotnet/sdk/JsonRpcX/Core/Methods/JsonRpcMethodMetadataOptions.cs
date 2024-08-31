using System.Reflection;

namespace JsonRpcX.Core.Methods;

/// <summary>
/// Internal options for initializing JSON RPC functionality.
/// </summary>
internal class JsonRpcMethodMetadataOptions
{
    public required Type Type { get; init; }

    /// <summary>
    /// Method names and their invocation metadata.
    /// </summary>
    // TODO: Same method name in multiple JSON RPC APIs?
    public required Dictionary<string, MethodInfo> Methods { get; init; }
}
