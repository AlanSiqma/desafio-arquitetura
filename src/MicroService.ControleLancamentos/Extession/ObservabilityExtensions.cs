using MassTransit.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MicroService.ControleLancamentos.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var endpoint =
            configuration["OpenTelemetry:Endpoint"]
            ?? "http://otel-collector:4317";

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: "controle-lancamentos",
                    serviceVersion: "1.0.0");
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                    });
            });

        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(options =>
            {
                options.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(endpoint);
                });
            });
        });

        return services;
    }
}