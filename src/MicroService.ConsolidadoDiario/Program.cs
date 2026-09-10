using Data.MongoDb.Context;
using MicroService.ConsolidadoDiario.Extensions;
using MicroService.ConsolidadoDiario.Security;
using MicroService.ConsolidadoDiario.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Security.Claims;


namespace MicroService.ConsolidadoDiario;
public class Program
{
    public static void Main(string[] args)
    {
        BsonSerializer.RegisterSerializer(
            new GuidSerializer(GuidRepresentation.Standard));

        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);


        var app = builder.Build();

        ConfigurePipeline(app);
        ConfigureEndpoints(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services
          .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
              options.Authority =
                  builder.Configuration["Keycloak:Authority"];

              options.MetadataAddress =
                  builder.Configuration["Keycloak:MetadataAddress"];

              options.Audience =
                  builder.Configuration["Keycloak:Audience"];

              options.RequireHttpsMetadata = false;

              options.TokenValidationParameters =
                  new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidIssuer =
                          builder.Configuration["Keycloak:Authority"],

                      ValidateAudience = true,
                      ValidAudience =
                          builder.Configuration["Keycloak:Audience"],

                      ValidateLifetime = true,
                      RoleClaimType = ClaimTypes.Role
                  };
          });
        builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
        builder.Services.AddApplicationMongo(builder.Configuration);
        builder.Services.AddScoped<ConsolidadoDiarioService>();
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "ConsolidadoRead",
                policy => policy.RequireRole("consolidado.read"));

            options.AddPolicy(
                "ConsolidadoWrite",
                policy => policy.RequireRole("consolidado.write"));
        });
        builder.Services.AddOpenApi();
        builder.AddObservability();
        builder.Services.AddApplicationHealthChecks();
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.MapOpenApi();
        app.MapHealthChecks("/health");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    private static void ConfigureEndpoints(WebApplication app)
    {
        app.MapGet("/consolidado-diario/{data:datetime}", async (
            DateTime data,
            ConsolidadoDiarioService service) =>
        {
            var consolidado = await service.ObterAsync(data);

            return consolidado is null
                ? Results.NotFound()
                : Results.Ok(consolidado);
        }).RequireAuthorization("ConsolidadoRead");


        app.MapPost("/consolidado-diario/{data:datetime}/gerar", async (
            DateTime data,
            ConsolidadoDiarioService service) =>
        {
            var consolidado = await service.GerarAsync(data);

            return Results.Ok(consolidado);
        }).RequireAuthorization("ConsolidadoWrite");
    }
}