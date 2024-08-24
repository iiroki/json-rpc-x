using System.Reflection;
using JsonRpcX.Methods;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodHandler2(IJsonRpcMethodHandler handler, MethodInfo method) : IJsonRpcMethodHandler2
{
    public IJsonRpcMethodHandler Handler { get; } = handler;

    public MethodInfo Method { get; } = method;
}
