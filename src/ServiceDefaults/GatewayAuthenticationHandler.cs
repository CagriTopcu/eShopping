using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServiceDefaults;

public sealed class GatewayAuthenticationOptions : AuthenticationSchemeOptions { }

public sealed class GatewayAuthenticationHandler : AuthenticationHandler<GatewayAuthenticationOptions>
{
    public GatewayAuthenticationHandler(
        IOptionsMonitor<GatewayAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var rolesHeader = Request.Headers["X-User-Roles"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };

        if (!string.IsNullOrWhiteSpace(rolesHeader))
        {
            foreach (var role in rolesHeader.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }
        }

        var identity = new ClaimsIdentity(claims, AuthExtensions.GatewayHeadersScheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), AuthExtensions.GatewayHeadersScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
