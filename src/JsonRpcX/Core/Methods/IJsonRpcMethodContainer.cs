using System.Collections.Immutable;
using System.Reflection;

namespace JsonRpcX.Core.Methods;

internal interface IJsonRpcMethodContainer
{
    ImmutableDictionary<string, MethodInfo> Methods { get; }
}
