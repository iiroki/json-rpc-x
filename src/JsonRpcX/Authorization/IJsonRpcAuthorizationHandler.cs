using JsonRpcX.Domain.Models;
using JsonRpcX.Exceptions;

namespace JsonRpcX.Authorization;

public interface IJsonRpcAuthorizationHandler
{
    /// <summary>
    /// Authorizes the JSON RPC request within the context.<br />
    /// <br />
    /// If authorization fails, the handler should throw an authorization expection.
    /// </summary>
    /// <exception cref="JsonRpcAuthorizationExpection" />
    Task HandleAsync(JsonRpcContext ctx, CancellationToken ct = default);
}
