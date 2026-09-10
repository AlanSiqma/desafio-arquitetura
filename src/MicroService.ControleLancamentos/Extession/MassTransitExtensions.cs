using Domain.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MicroService.ControleLancamentos.Extensions;

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
            x.AddEntityFrameworkOutbox<AppDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

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

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}