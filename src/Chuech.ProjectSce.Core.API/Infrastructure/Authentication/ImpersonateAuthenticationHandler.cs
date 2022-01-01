using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Authentication;

public class ImpersonateAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ImpersonateAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = GetUserId();
        if (userId is not { } realUserId)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        var claims = new[] { new Claim("public_id", realUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Impersonate");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Impersonate");
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private int? GetUserId()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Impersonate "))
        {
            return null;
        }

        if (int.TryParse(authHeader.AsSpan("Impersonate ".Length), out var value))
        {
            return value;
        }

        return null;
    }
}