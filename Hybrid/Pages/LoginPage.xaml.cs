using CommunityToolkit.Maui.Alerts;
using Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Hybrid;

public partial class LoginPage : ContentPage
{
    private readonly IOptions<SiteSettings> _siteSettings;
    private readonly IServiceProvider _serviceProvider;
    private string? Email { get; set; }
    private string? Token { get; set; }

    public LoginPage(IOptions<SiteSettings> siteSettings, IServiceProvider serviceProvider)
    {
        // https://stackoverflow.com/questions/74269299/login-page-for-net-maui/74291417#74291417
        InitializeComponent();

        _siteSettings = siteSettings;
        _serviceProvider = serviceProvider;
    }

    async void OnTokenEntryCompleted(object sender, EventArgs e)
    {
        Token = ((Entry)sender).Text;
    }

    async void OnEmailEntryCompleted(object sender, EventArgs e)
    {
        Email = ((Entry)sender).Text;
    }

    async void OnLoginClicked(object sender, EventArgs args)
    {
        if (Application.Current != null && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Token))
        {
            Preferences.Default.Set(nameof(PreferenceKeys.Email), Email);
            Preferences.Default.Set(nameof(PreferenceKeys.Token), Token);

            Application.Current.MainPage = _serviceProvider.GetRequiredService<AppShell>();
            _ = Toast.Make("Logged in.").Show();
        }
        else
        {
            _ = Toast.Make("Something went wrong.").Show();
        }
    }

    async void OnRegisterClicked(object sender, EventArgs args)
    {
        await Browser.Default.OpenAsync(_siteSettings.Value.WebLink, BrowserLaunchMode.SystemPreferred);
    }
}