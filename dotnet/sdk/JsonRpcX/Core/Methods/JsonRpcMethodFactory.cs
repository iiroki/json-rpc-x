using System.Collections.Immutable;
using System.Reflection;
using JsonRpcX.Exceptions;
using JsonRpcX.Methods;
using JsonRpcX.Models;
using JsonRpcX.Options;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodFactory(IEnumerable<JsonRpcInternalMethodOptions> opt) : IJsonRpcMethodFactory
{
    public ImmutableDictionary<string, MethodInfo> Methods { get; } =
        opt.SelectMany(o => o.Methods).ToImmutableDictionary();

    public IJsonRpcMethodHandler2 CreateHandler(IServiceScope scope, string method, JsonRpcContext ctx)
    {
        // 1. Find method handler for the method
        var key = JsonRpcConstants.DiKeyPrefix + method;
        var handler =
            scope.ServiceProvider.GetKeyedService<IJsonRpcMethodHandler>(key)
            ?? throw new JsonRpcErrorException(
                ctx,
                (int)JsonRpcConstants.ErrorCode.MethodNotFound,
                $"Method not found: {method}"
            );

        // 2. Find method invocation metadata for the method
        if (!Methods.TryGetValue(method, out var methodMetadata))
        {
            throw new JsonRpcErrorException(
                ctx,
                (int)JsonRpcConstants.ErrorCode.InternalError,
                $"Method invocation metadata not found for method: {method}"
            );
        }

        // 3. Validate that the handler and method invocation metadata types match
        if (handler.GetType() != methodMetadata.DeclaringType)
        {
            throw new JsonRpcErrorException(
                ctx,
                (int)JsonRpcConstants.ErrorCode.InternalError,
                $"Handler <-> Method type mismatch: {handler.GetType().FullName} != {methodMetadata.DeclaringType}"
            );
        }

        // 4. Create the internal method handler
        return new JsonRpcMethodHandler2(handler, methodMetadata);
    }
}
