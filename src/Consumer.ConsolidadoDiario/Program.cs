using Consumer.ConsolidadoDiario.Consumers;
using Consumer.ConsolidadoDiario.Extensions;
using Consumer.ConsolidadoDiario.Observability;
using Data.MongoDb.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using static Consumer.ConsolidadoDiario.Extensions.ObservabilityExtensions;

BsonSerializer.RegisterSerializer(
    new GuidSerializer(GuidRepresentation.Standard));

using var activity =
    ConsumerActivitySource.Source.StartActivity(
        "LancamentoCriadoEvent.Process");
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<LancamentoCriadoConsumer>();
builder.Services.AddScoped<LancamentoAlteradoConsumer>();
builder.Services.AddScoped<LancamentoExcluidoConsumer>();

builder.AddObservability();
builder.Services.AddApplicationMongo(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddApplicationHealthChecks();


var host = builder.Build();

await host.RunAsync();