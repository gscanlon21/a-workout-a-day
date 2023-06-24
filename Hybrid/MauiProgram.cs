using App;
using CommunityToolkit.Maui;
using Core.Models.Options;
using Microsoft.Extensions.Configuration;
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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa_solid.ttf", "FontAwesome");
            }).UseMauiCommunityToolkit();

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddLibServices();

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

            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<AppShellViewModel>();

            builder.Services.AddTransient<AuthShell>();
            builder.Services.AddTransient<AuthShellViewModel>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<NewsletterPage>();

            return builder.Build();
        }
    }
}