using JsonRpcX.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace JsonRpcX.Exceptions;

public class JsonRpcAuthorizationExpection(AuthorizationFailure failure, string msg = "Authorization failed")
    : JsonRpcException(msg)
{
    public AuthorizationFailure Failure { get; } = failure;
}
