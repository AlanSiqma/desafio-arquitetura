using MassTransit.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Consumer.ConsolidadoDiario.Extensions;

public static class ObservabilityExtensions
{
    public static IHostApplicationBuilder AddObservability(
        this IHostApplicationBuilder builder)
    {
        var endpoint =
            builder.Configuration["OpenTelemetry:Endpoint"]
            ?? "http://otel-collector:4317";

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: "consumer-consolidado-diario",
                    serviceVersion: "1.0.0");
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddSource("Consumer.ConsolidadoDiario")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                    });
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(endpoint);
            });
        });

        return builder;
    }
}