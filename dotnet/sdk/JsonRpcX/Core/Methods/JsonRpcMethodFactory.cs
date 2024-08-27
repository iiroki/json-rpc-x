using System.Text.Json;
using JsonRpcX.Exceptions;
using JsonRpcX.Methods;

namespace JsonRpcX.Core.Methods;

internal class JsonRpcMethodFactory(
    IServiceProvider services,
    IJsonRpcMethodContainer container,
    JsonSerializerOptions? jsonOptions = null
) : IJsonRpcMethodFactory
{
    private readonly IServiceProvider _services = services;
    private readonly IJsonRpcMethodContainer _container = container;
    private readonly JsonSerializerOptions? _jsonOptions = jsonOptions;

    public IJsonRpcMethodInvoker CreateInvocation(string method)
    {
        // 1. Find method handler for the method
        var key = JsonRpcConstants.DiKeyPrefix + method;

        var handler =
            _services.GetKeyedService<IJsonRpcMethodHandler>(key)
            ?? throw new JsonRpcErrorException(
                (int)JsonRpcConstants.ErrorCode.MethodNotFound,
                $"Method not found: {method}"
            );

        // 2. Find method invocation metadata for the method
        if (!_container.Methods.TryGetValue(method, out var methodMetadata))
        {
            throw new JsonRpcErrorException(
                (int)JsonRpcConstants.ErrorCode.InternalError,
                $"Method invocation metadata not found for method: {method}"
            );
        }

        // 3. Validate that the handler and method invocation metadata types match
        if (handler.GetType() != methodMetadata.DeclaringType)
        {
            throw new JsonRpcErrorException(
                (int)JsonRpcConstants.ErrorCode.InternalError,
                $"Handler <-> Method type mismatch: {handler.GetType().FullName} != {methodMetadata.DeclaringType}"
            );
        }

        // 4. Create the method invoker
        return new JsonRpcMethodInvoker(handler, methodMetadata, _jsonOptions);
    }
}
