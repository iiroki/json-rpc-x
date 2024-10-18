using JsonRpcX.Api.Services;
using JsonRpcX.Attributes;
using JsonRpcX.Controllers;

namespace JsonRpcX.Api.Controllers;

public class JsonRpcExampleController(IGreeterService greeter) : IJsonRpcController
{
    private readonly IGreeterService _greeter = greeter;

    [JsonRpcMethod]
    public string Hello(string name) => _greeter.SayHello(name);
}
