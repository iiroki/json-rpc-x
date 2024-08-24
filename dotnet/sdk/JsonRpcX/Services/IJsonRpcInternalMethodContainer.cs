using System.Collections.Immutable;
using System.Reflection;

namespace JsonRpcX.Services;

internal interface IJsonRpcInternalMethodContainer
{
    ImmutableDictionary<string, MethodInfo> Methods { get; }
}
