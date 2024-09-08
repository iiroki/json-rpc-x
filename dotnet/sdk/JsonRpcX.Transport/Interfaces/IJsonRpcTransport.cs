using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Transport.Interfaces;

public interface IJsonRpcTransport
{
    public RequestDelegate Delegate { get; }

    // TODO
}
