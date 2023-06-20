using App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorApp(this IServiceCollection services)
    {
        //services.AddHttpClient<IWeatherService, WeatherService>(httpClient => httpClient.BaseAddress = new Uri(baseUri));
        services.AddSingleton<AppState>();
        services.AddSingleton<DisplayHelper>();
        services.AddTransient<NewsletterService>();
        services.AddTransient<FootnoteService>();
        services.AddTransient<UserService>();
        return services;
    }
}