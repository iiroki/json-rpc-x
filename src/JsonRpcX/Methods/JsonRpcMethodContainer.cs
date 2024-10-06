using System.Collections.Immutable;

namespace JsonRpcX.Methods;

internal class JsonRpcMethodContainer(JsonRpcMethodBuilder container) : IJsonRpcMethodContainer
{
    public ImmutableDictionary<string, JsonRpcMethodInfo> Methods { get; } =
        container.Methods.ToImmutableDictionary(m => m.Name, m => m);
}
