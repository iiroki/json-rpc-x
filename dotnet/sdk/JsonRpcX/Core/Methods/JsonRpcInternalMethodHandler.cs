using System.Reflection;
using JsonRpcX.Methods;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodHandlerInvocation(IJsonRpcMethodHandler handler, MethodInfo method)
    : IJsonRpcMethodInvocation
{
    public IJsonRpcMethodHandler Handler { get; } = handler;

    public MethodInfo Method { get; } = method;
}
