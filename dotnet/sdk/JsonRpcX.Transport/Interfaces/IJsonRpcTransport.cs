using Microsoft.AspNetCore.Http;

namespace JsonRpcX.Transport.Interfaces;

public interface IJsonRpcTransport
{
    public string Type { get; }

    /// <summary>
    /// HTTP request delegate for receiving data.
    /// </summary>
    public RequestDelegate Delegate { get; }
}
