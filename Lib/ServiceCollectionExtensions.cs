using Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorApp(this IServiceCollection services)
    {
        services.AddScoped<AppState>();
        services.AddScoped<DisplayHelper>();

        services.AddTransient<NewsletterService>();
        services.AddTransient<FootnoteService>();
        services.AddTransient<UserService>();

        return services;
    }
}