using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MicroService.ConsolidadoDiario.Extensions;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder)
    {
        var endpoint =
            builder.Configuration["OpenTelemetry:Endpoint"]
            ?? "http://otel-collector:4317";

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: "consolidado-diario",
                    serviceVersion: "1.0.0");
            })
            .WithTracing(tracing =>
            {
                tracing
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