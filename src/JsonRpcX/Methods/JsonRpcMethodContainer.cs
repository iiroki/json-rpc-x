using System.Collections.Immutable;

namespace JsonRpcX.Methods;

internal class JsonRpcMethodContainer(IEnumerable<JsonRpcMethodBuilder> containers) : IJsonRpcMethodContainer
{
    public ImmutableDictionary<string, JsonRpcMethodInfo> Methods { get; } =
        containers.SelectMany(c => c.Methods).ToImmutableDictionary(m => m.Name, m => m);
}
