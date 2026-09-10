using Data.MongoDb.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Consumer.ConsolidadoDiario.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>(
                "mongodb",
                failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _mongoDbContext;

    public MongoDbHealthCheck(MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _mongoDbContext
                .PingAsync(cancellationToken);

            return HealthCheckResult.Healthy(
                "MongoDB está disponível.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "MongoDB está indisponível.",
                ex);
        }
    }
}