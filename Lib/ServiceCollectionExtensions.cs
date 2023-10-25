using Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLibServices(this IServiceCollection services)
    {
        services.AddScoped<AppState>();
        services.AddScoped<DisplayHelper>();

        services.AddTransient<NewsletterService>();
        services.AddTransient<UserService>();

        return services;
    }
}