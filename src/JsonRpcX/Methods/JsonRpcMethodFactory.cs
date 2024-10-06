using System.Text.Json;
using JsonRpcX.Domain.Exceptions;
using JsonRpcX.Extensions;

namespace JsonRpcX.Methods;

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
        // 1. Find controller for the method
        var controller = _services.GetJsonRpcController(method) ?? throw new JsonRpcMethodNotFoundException(method);

        // 2. Find method invocation metadata for the method
        if (!_container.Methods.TryGetValue(method, out var methodMetadata))
        {
            throw new JsonRpcException($"Method invocation metadata not found for method: {method}");
        }

        // 3. Validate that the controller and method invocation metadata types match
        if (controller.GetType() != methodMetadata.DeclaringType)
        {
            throw new JsonRpcException(
                $"Controller/method type mismatch: {controller.GetType().FullName} != {methodMetadata.DeclaringType?.FullName}"
            );
        }

        // 4. Create the method invoker
        return new JsonRpcMethodInvoker(controller, methodMetadata, _jsonOptions);
    }
}
