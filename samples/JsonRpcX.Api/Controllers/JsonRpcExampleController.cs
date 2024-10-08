using JsonRpcX.Attributes;
using JsonRpcX.Controllers;

namespace JsonRpcX.Api.Controllers;

public class JsonRpcExampleController : IJsonRpcController
{
    [JsonRpcMethod]
    public string Hello(string name) => $"Hello, {name}!";
}
