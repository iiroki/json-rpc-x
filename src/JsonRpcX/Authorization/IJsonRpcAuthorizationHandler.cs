using JsonRpcX.Domain.Models;

namespace JsonRpcX.Authorization;

public interface IJsonRpcAuthorizationHandler
{
    Task HandleAsync(JsonRpcContext ctx, CancellationToken ct = default);
}
