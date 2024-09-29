using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Extensions;

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

    public IJsonRpcMethodInvoker Create(string method)
    {
        // 1. Find method handler for the method
        var handler = _services.GetJsonRpcController(method) ?? throw new JsonRpcMethodNotFoundException(method);

        // 2. Find method invocation metadata for the method
        if (!_container.Methods.TryGetValue(method, out var methodMetadata))
        {
            throw new JsonRpcException($"Method invocation metadata not found for method: {method}");
        }

        // 3. Validate that the handler and method invocation metadata types match
        if (handler.GetType() != methodMetadata.DeclaringType)
        {
            throw new JsonRpcException(
                $"Handler <-> Method type mismatch: {handler.GetType().FullName} != {methodMetadata.DeclaringType}"
            );
        }

        // 4. Create the method invoker
        return new JsonRpcMethodInvoker(handler, methodMetadata, _jsonOptions);
    }
}
