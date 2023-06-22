using App;
using CommunityToolkit.Maui;
using Core.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/* Unmerged change from project 'Hybrid (net7.0-android)'
Before:
using App;
After:
using Microsoft.Extensions.Configuration;
*/

/* Unmerged change from project 'Hybrid (net7.0-ios)'
Before:
using App;
After:
using Microsoft.Extensions.Configuration;
*/

/* Unmerged change from project 'Hybrid (net7.0-windows10.0.19041.0)'
Before:
using App;
After:
using Microsoft.Extensions.Configuration;
*/
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Hybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            }).UseMauiCommunityToolkit();

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddBlazorApp();

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(App).Assembly.GetName().Name}.client.appsettings.json");
            if (stream != null) 
            {
                builder.Configuration.AddConfiguration(new ConfigurationBuilder().AddJsonStream(stream).Build());
            } 

            builder.Services.Configure<SiteSettings>(
                builder.Configuration.GetSection("SiteSettings")
            );

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<HttpClient>();

            return builder.Build();
        }
    }
}