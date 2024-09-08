using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Transport.Interfaces;

public interface IJsonRpcTransport
{
    public string Type { get; }

    public RequestDelegate Delegate { get; }

    // TODO
}
