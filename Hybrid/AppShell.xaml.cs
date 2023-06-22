using Microsoft.Maui.Controls;

namespace Hybrid
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(NewsletterPage), typeof(NewsletterPage));
        }
    }
}