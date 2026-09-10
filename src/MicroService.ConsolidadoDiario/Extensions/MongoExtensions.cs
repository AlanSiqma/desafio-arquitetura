using Data.MongoDb.Context;
using Domain.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace MicroService.ConsolidadoDiario.Extensions;

public static class MongoExtensions
{
    public static IServiceCollection AddApplicationMongo(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<MongoDbContext>();

        services.AddSingleton<IMongoCollection<LancamentoDocument>>(
            provider =>
                provider
                    .GetRequiredService<MongoDbContext>()
                    .Lancamentos);

        services.AddSingleton<IMongoCollection<ConsolidadoDiarioDocument>>(
            provider =>
                provider
                    .GetRequiredService<MongoDbContext>()
                    .ConsolidadosDiarios);

        return services;
    }
}