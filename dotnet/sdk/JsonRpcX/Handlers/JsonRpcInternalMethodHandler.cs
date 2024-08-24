using System.Reflection;

namespace JsonRpcX.Handlers;

internal class JsonRpcInternalMethodHandler(IJsonRpcMethodHandler handler, MethodInfo method)
    : IJsonRpcInternalMethodHandler
{
    public IJsonRpcMethodHandler Handler { get; } = handler;

    public MethodInfo Method { get; } = method;
}
