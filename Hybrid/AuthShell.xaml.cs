using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Windows.Input;

namespace Hybrid;

public partial class AuthShell : Shell
{
    /// <param name="serviceProvider">https://github.com/dotnet/maui/issues/11485</param>
    public AuthShell(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        //Routing.RegisterRoute(nameof(NewsletterPage), typeof(NewsletterPage));

        BindingContext = serviceProvider.GetRequiredService<AppShellViewModel>();
    }
}

public class AuthShellViewModel
{
    /// <summary>
    /// Used in the view.
    /// </summary>
    public IOptions<SiteSettings> SiteSettings { get; private set; }

    public AuthShellViewModel(IOptions<SiteSettings> siteSettings)
    {
        SiteSettings = siteSettings;
    }

    public ICommand BrowserCommand { private set; get; } = new Command<string>(async (arg) =>
    {
        await Browser.Default.OpenAsync(arg, BrowserLaunchMode.SystemPreferred);
    });
}