namespace JsonRpcX.Api.Services;

internal class GreeterService : IGreeterService
{
    public string SayHello(string name) => $"Hello, {name}!";
}
