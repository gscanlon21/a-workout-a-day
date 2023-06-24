using CommunityToolkit.Maui.Alerts;

namespace Hybrid
{
    public partial class LoginPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private string? Email { get; set; }
        private string? Token { get; set; }

        public LoginPage(IServiceProvider serviceProvider)
        {
            // https://stackoverflow.com/questions/74269299/login-page-for-net-maui/74291417#74291417
            InitializeComponent();

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
            Preferences.Default.Set(nameof(PreferenceKeys.Email), Email);
            Preferences.Default.Set(nameof(PreferenceKeys.Token), Token);

            if (Application.Current != null)
            {
                _ = Toast.Make("Logged in.").Show();
                Application.Current.MainPage = _serviceProvider.GetRequiredService<AppShell>();
            }
        }
    }
}