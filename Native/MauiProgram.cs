using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Native.Services;
using Native.ViewModels;
using Native.Controls;

#if ANDROID
using Native.Platforms.Android.Handlers;
#endif

#if IOS
using Native.Platforms.iOS.Handlers;
#endif

#if WINDOWS
using Native.Platforms.Windows.Handlers;
#endif

namespace Native
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewHandler));
            }).UseMauiCommunityToolkit();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddSingleton<LoginPage>();

            builder.Services.AddTransient<NewsletterPage>();
            builder.Services.AddTransient<NewsletterViewModel>();

            builder.Services.AddTransient<NewsletterWebPage>();
            builder.Services.AddTransient<NewsletterWebViewModel>();

            builder.Services.AddSingleton<RestService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}