using MicroService.ControleLancamentos.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MicroService.ControleLancamentos.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddApplicationSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority =
                    configuration["Keycloak:Authority"];

                options.MetadataAddress =
                    configuration["Keycloak:MetadataAddress"];

                options.Audience =
                    configuration["Keycloak:Audience"];

                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer =
                            configuration["Keycloak:Authority"],

                        ValidateAudience = true,
                        ValidAudience =
                            configuration["Keycloak:Audience"],

                        ValidateLifetime = true,

                        RoleClaimType = ClaimTypes.Role
                    };
            });

        services.AddTransient<
            IClaimsTransformation,
            KeycloakRolesClaimsTransformation>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "LancamentosRead",
                policy => policy.RequireRole("lancamentos.read"));

            options.AddPolicy(
                "LancamentosWrite",
                policy => policy.RequireRole("lancamentos.write"));
        });

        return services;
    }
}