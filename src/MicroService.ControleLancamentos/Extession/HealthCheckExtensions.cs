using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MicroService.ControleLancamentos.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>(
                "postgres",
                failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}

public class PostgresHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public PostgresHealthCheck(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _db.Database
                .CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy(
                    "PostgreSQL está disponível.")
                : HealthCheckResult.Unhealthy(
                    "PostgreSQL está indisponível.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Não foi possível conectar ao PostgreSQL.",
                ex);
        }
    }
}