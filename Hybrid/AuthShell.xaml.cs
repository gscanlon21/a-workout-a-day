using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Windows.Input;

namespace Hybrid
{
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
        public IOptions<SiteSettings> SiteSettings { get; set; }

        public ICommand SourceCommand { private set; get; }

        public AuthShellViewModel(IOptions<SiteSettings> siteSettings)
        {
            SiteSettings = siteSettings;
            SourceCommand = new Command<string>(async (string arg) =>
            {
                await Browser.Default.OpenAsync(arg, BrowserLaunchMode.SystemPreferred);
            });
        }
    }
}