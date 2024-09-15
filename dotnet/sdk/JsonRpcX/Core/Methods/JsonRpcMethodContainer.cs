using System.Collections.Immutable;
using System.Reflection;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodContainer(IEnumerable<JsonRpcMethodMetadataBuilder> builders) : IJsonRpcMethodContainer
{
    public ImmutableDictionary<string, MethodInfo> Methods { get; } =
        builders.SelectMany(o => o.Methods).ToImmutableDictionary();
}
