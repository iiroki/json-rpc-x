namespace JsonRpcX.Core.Endpoints;

internal interface IJsonRpcEndpointFactory
{
    RequestDelegate Create();
}
