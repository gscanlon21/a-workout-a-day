using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace App;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorApp(this IServiceCollection services, string baseUri)
    {
        //services.AddHttpClient<IWeatherService, WeatherService>(httpClient => httpClient.BaseAddress = new Uri(baseUri));
        //services.AddSingleton<AppState>();
        //services.AddSingleton<DisplayHelper>();
        return services;
    }
}