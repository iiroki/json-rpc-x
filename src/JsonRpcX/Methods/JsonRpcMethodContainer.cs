using System.Collections.Immutable;
using JsonRpcX.Domain.Exceptions;

namespace JsonRpcX.Methods;

internal class JsonRpcMethodContainer(IEnumerable<JsonRpcMethodBuilder> containers) : IJsonRpcMethodContainer
{
    public ImmutableDictionary<string, JsonRpcMethodInfo> Methods { get; } =
        containers.SelectMany(c => c.Methods).ToImmutableDictionary(m => m.Name, m => m);

    public JsonRpcMethodInfo Get(string method) =>
        Methods.TryGetValue(method, out var m) ? m : throw new JsonRpcMethodNotFoundException(method);
}
