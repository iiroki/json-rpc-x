using System.Reflection;

namespace JsonRpcX.Methods;

/// <summary>
/// Internal options for initializing JSON RPC functionality.
/// </summary>
internal class JsonRpcMethodMetadataBuilder
{
    /// <summary>
    /// Method names and their invocation metadata.
    /// </summary>
    public required Dictionary<string, MethodInfo> Methods { get; init; }
}
