using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using App;

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
            builder.Services.AddBlazorApp("https://minimalweather20210428173256.azurewebsites.net/");

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            //builder.Services.AddSingleton<WeatherForecastService>();

            return builder.Build();
        }
    }
}