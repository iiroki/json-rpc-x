using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace JsonRpcX.Api.Authorization;

/// <summary>
/// Very very dumb authentication handler that just reads authorization data from "Authorization" header.<br />
/// <br />
/// Example: name / role1, role2
/// </summary>
public class ExampleAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ExampleAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder)
    {
        // NOP
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string scheme = nameof(ExampleAuthenticationHandler);
        const string nameClaim = nameof(nameClaim);
        const string roleClaim = nameof(roleClaim);

        var auth = Context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(auth))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var parts = auth.Split('/').Select(p => p.Trim()).ToList();
        if (parts.Count < 2)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid header value"));
        }

        List<Claim> claims = [new(nameClaim, parts[0])];
        foreach (var r in parts[1].Split(',').Select(r => r.Trim()).ToList())
        {
            claims.Add(new(roleClaim, r));
        }

        var identity = new ClaimsIdentity(claims, scheme, nameClaim, roleClaim);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
