using JsonRpcX.Methods;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Extensions;

public static class JsonRpcExtensions
{
    private const string KeyPrefix = "json-rpc@";

    public static IServiceCollection AddJsonRpcMethod(
        this IServiceCollection services,
        string method,
        Type handlerType
    ) => services.AddKeyedScoped(typeof(IJsonRpcMethodHandler), ToMethodKey(method), handlerType);

    public static IJsonRpcMethodHandler? GetJsonRpcMethod(this IServiceProvider services, string method) =>
        services.GetKeyedService<IJsonRpcMethodHandler>(ToMethodKey(method));

    private static string ToMethodKey(string method) => KeyPrefix + method;
}
