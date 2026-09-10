using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace MicroService.ControleLancamentos.Security;

public sealed class KeycloakRolesClaimsTransformation
    : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(
        ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;

        if (identity is null || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        var realmAccessClaim =
            identity.FindFirst("realm_access");

        if (realmAccessClaim is null)
            return Task.FromResult(principal);

        using var document =
            JsonDocument.Parse(realmAccessClaim.Value);

        if (!document.RootElement.TryGetProperty(
                "roles",
                out var roles))
        {
            return Task.FromResult(principal);
        }

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString();

            if (!string.IsNullOrWhiteSpace(roleName) &&
                !identity.HasClaim(
                    ClaimTypes.Role,
                    roleName))
            {
                identity.AddClaim(
                    new Claim(
                        ClaimTypes.Role,
                        roleName));
            }
        }

        return Task.FromResult(principal);
    }
}