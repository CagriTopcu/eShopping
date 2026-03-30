using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Gateway.API.Auth;

internal sealed class KeycloakRolesClaimsTransformation(
    ILogger<KeycloakRolesClaimsTransformation> logger) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim is null)
        {
            return Task.FromResult(principal);
        }

        var cloned = principal.Clone();
        var identity = (ClaimsIdentity)cloned.Identity!;

        try
        {
            using var doc = JsonDocument.Parse(realmAccessClaim.Value);
            if (!doc.RootElement.TryGetProperty("roles", out var rolesElement))
            {
                return Task.FromResult(cloned);
            }

            foreach (var role in rolesElement.EnumerateArray())
            {
                var roleValue = role.GetString();
                if (!string.IsNullOrWhiteSpace(roleValue) &&
                    !identity.HasClaim(ClaimTypes.Role, roleValue))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "Failed to parse realm_access claim for user {Sub}. No Keycloak roles will be mapped.",
                principal.FindFirst("sub")?.Value ?? "unknown");
        }

        return Task.FromResult(cloned);
    }
}
