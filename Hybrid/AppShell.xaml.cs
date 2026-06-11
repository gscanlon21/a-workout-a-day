using CommunityToolkit.Maui.Alerts;
using Hybrid.Database;
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

public class AppShellViewModel
{
    /// <summary>
    /// Used in the view.
    /// </summary>
    public IOptions<SiteSettings> SiteSettings { get; private set; }

    public AppShellViewModel(IOptions<SiteSettings> siteSettings, UserPreferences preferences, IServiceProvider serviceProvider)
    {
        SiteSettings = siteSettings;

        PreferencesCommand = new Command(async () =>
        {
            await Browser.Default.OpenAsync($"{siteSettings.Value.WebLink}/u/{Uri.EscapeDataString(preferences.Email.Value)}?token={Uri.EscapeDataString(preferences.Token.Value)}", BrowserLaunchMode.SystemPreferred);
        });

        LogoutCommand = new Command(() =>
        {
            Preferences.Default.Clear();

            if (Application.Current != null)
            {
                _ = Toast.Make("Logged out.").Show();
                Application.Current.MainPage = serviceProvider.GetRequiredService<AuthShell>();
            }
        });
    }

    public ICommand LogoutCommand { private set; get; }
    public ICommand PreferencesCommand { private set; get; }

    public ICommand BrowserCommand { private set; get; } = new Command<string>(async (arg) =>
    {
        await Browser.Default.OpenAsync(arg, BrowserLaunchMode.SystemPreferred);
    });
}
