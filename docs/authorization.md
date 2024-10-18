# JSON RPC X - Authorization

## Authorize JSON RPC methods

JSON RPC method authorization can be controlled with `[Authorize]` and `[AllowAnonymous]` attributes.

By default, _JSON RPC X_ supports `Roles` and `Policy` properties of `[Authorize]`.

The authorization attributes can be set in controller or method level.

```cs
[Authorize] // Requires an authenticated user for all methods
public class JsonRpcExampleController : IJsonRpcController
{
    [JsonRpcMethod]
    public string Authenticated() => "Requires authentication";

    [JsonRpcMethod]
    [Authorize(Roles = "example-role")]
    public string Role() => "Requires 'example-role' role";

    [JsonRpcMethod]
    [Authorize(Policy = "example-policy")]
    public string Policy() => "Requires 'example-policy' to succeed";

    [JsonRpcMethod]
    [AllowAnonymous] // Overrides the controller level authorization
    public string Anonymous() => "No authorization";
}
```

**NOTES:**
- JSON RPC pipeline uses `IJsonRpcAuthorizationHandler` to perform the authorization checks.
- The default `IJsonRpcAuthorizationHandler` implementation uses .NET's `IAuthorizationService`.
- Read .NET authorization documentation for more information:
    - TODO

## Custom JSON RPC method authorization

_JSON RPC X_ provides the following options to configure the method authorization:
- Replacing the `IJsonRpcAuthorizationHandler` implementation.
    - Use `IJsonRpcMethodContainer` to access the method information in your custom authorization handler.
- Providing a custom `JsonRpcMethodOptions.AuthorizationResolver` for resolving authorization data from the methods.
