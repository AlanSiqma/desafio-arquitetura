using Consumer.ConsolidadoDiario.Consumers;
using Domain.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Consumer.ConsolidadoDiario.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMQSettings>(
            configuration.GetSection("RabbitMQ"));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<LancamentoCriadoConsumer>();
            x.AddConsumer<LancamentoAlteradoConsumer>();
            x.AddConsumer<LancamentoExcluidoConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = context
                    .GetRequiredService<IOptions<RabbitMQSettings>>()
                    .Value;

                cfg.Host(
                    settings.Host,
                    (ushort)settings.Port,
                    settings.VirtualHost,
                    host =>
                    {
                        host.Username(settings.Username);
                        host.Password(settings.Password);
                    });

                cfg.ReceiveEndpoint("lancamento-criado", e =>
                {
                    e.UseMessageRetry(r =>
                    {
                        r.Interval(
                            3,
                            TimeSpan.FromSeconds(5));
                    });

                    e.ConfigureConsumer<LancamentoCriadoConsumer>(
                        context);
                });

                cfg.ReceiveEndpoint("lancamento-alterado", e =>
                {
                    e.UseMessageRetry(r =>
                    {
                        r.Interval(
                            3,
                            TimeSpan.FromSeconds(5));
                    });

                    e.ConfigureConsumer<LancamentoAlteradoConsumer>(
                        context);
                });

                cfg.ReceiveEndpoint("lancamento-excluido", e =>
                {
                    e.UseMessageRetry(r =>
                    {
                        r.Interval(
                            3,
                            TimeSpan.FromSeconds(5));
                    });

                    e.ConfigureConsumer<LancamentoExcluidoConsumer>(
                        context);
                });
            });
        });

        return services;
    }
}