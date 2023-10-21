using CommunityToolkit.Maui.Alerts;
using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Windows.Input;

namespace Hybrid;

public partial class AppShell : Shell
{
    /// <param name="serviceProvider">https://github.com/dotnet/maui/issues/11485</param>
    public AppShell(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        //Routing.RegisterRoute(nameof(NewsletterPage), typeof(NewsletterPage));

        BindingContext = serviceProvider.GetRequiredService<AppShellViewModel>();
    }
}

public class AppShellViewModel(IOptions<SiteSettings> siteSettings, IServiceProvider serviceProvider)
{
    /// <summary>
    /// Used in the view.
    /// </summary>
    public IOptions<SiteSettings> SiteSettings { get; set; } = siteSettings;

    public ICommand BrowserCommand { private set; get; } = new Command<string>(async (string arg) =>
        {
            await Browser.Default.OpenAsync(arg, BrowserLaunchMode.SystemPreferred);
        });
    public ICommand LogoutCommand { private set; get; } = new Command(() =>
        {
            Preferences.Default.Clear();

            if (Application.Current != null)
            {
                _ = Toast.Make("Logged out.").Show();
                Application.Current.MainPage = serviceProvider.GetRequiredService<AuthShell>();
            }
        });
}