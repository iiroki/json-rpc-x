using System.Collections.Immutable;
using System.Reflection;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodContainer(IEnumerable<JsonRpcMethodMetadataOptions> opt) : IJsonRpcMethodContainer
{
    public ImmutableDictionary<string, MethodInfo> Methods { get; } =
        opt.SelectMany(o => o.Methods).ToImmutableDictionary();
}
