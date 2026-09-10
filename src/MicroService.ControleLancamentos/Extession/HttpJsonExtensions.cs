using System.Text.Json.Serialization;

namespace MicroService.ControleLancamentos.Extensions;

public static class HttpJsonExtensions
{
    public static IServiceCollection AddApplicationJson(
        this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter());
        });

        return services;
    }
}