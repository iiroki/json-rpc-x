using JsonRpcX.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace JsonRpcX.Extensions;

public static class JsonRpcExtensions
{
    private const string KeyPrefix = "json-rpc@";

    public static IServiceCollection AddJsonRpcMethod(
        this IServiceCollection services,
        string method,
        Type controllerType
    ) => services.AddKeyedScoped(typeof(IJsonRpcController), ToMethodKey(method), controllerType);

    public static IJsonRpcController? GetJsonRpcController(this IServiceProvider services, string method) =>
        services.GetKeyedService<IJsonRpcController>(ToMethodKey(method));

    private static string ToMethodKey(string method) => KeyPrefix + method;
}
