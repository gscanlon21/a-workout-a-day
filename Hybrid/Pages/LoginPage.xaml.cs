using CommunityToolkit.Maui.Alerts;
using Core.Models.Options;
using Lib.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Hybrid;

public partial class LoginPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly UserService _userService;
    private string? Email { get; set; }
    private string? Token { get; set; }

    public IOptions<SiteSettings> SiteSettings { get; set; }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Xaml cannot bind to static property")]
    public ICommand OpenUrlCommand => new Command<string>(async (url) => await Browser.OpenAsync(url));

    public LoginPage(IOptions<SiteSettings> siteSettings, UserService userService, IServiceProvider serviceProvider)
    {
        // https://stackoverflow.com/questions/74269299/login-page-for-net-maui/74291417#74291417
        InitializeComponent();

        _userService = userService;
        _serviceProvider = serviceProvider;

        SiteSettings = siteSettings;

        BindingContext = this;
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
            var user = await _userService.GetUser(Email, Token);
            if (user.IsSuccessStatusCode && user.HasValue)
            {
                Preferences.Default.Set(nameof(PreferenceKeys.Email), Email);
                Preferences.Default.Set(nameof(PreferenceKeys.Token), Token);

                Application.Current.MainPage = _serviceProvider.GetRequiredService<AppShell>();
                _ = Toast.Make("Logged in.").Show();
            }
            else
            {
                _ = Toast.Make("Invalid sign-in.").Show();
            }
        }
        else
        {
            _ = Toast.Make("Something went wrong.").Show();
        }
    }

    async void OnRegisterClicked(object sender, EventArgs args)
    {
        await Browser.Default.OpenAsync(SiteSettings.Value.WebLink, BrowserLaunchMode.SystemPreferred);
    }
}