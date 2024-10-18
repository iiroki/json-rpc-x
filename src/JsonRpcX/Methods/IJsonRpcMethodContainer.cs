using System.Collections.Immutable;

namespace JsonRpcX.Methods;

internal interface IJsonRpcMethodContainer
{
    ImmutableDictionary<string, JsonRpcMethodInfo> Methods { get; }

    JsonRpcMethodInfo Get(string method);
}
