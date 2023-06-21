using CommunityToolkit.Maui.Alerts;

namespace Hybrid
{
    public partial class LoginPage : ContentPage
    {
        int count = 0;

        public LoginPage()
        {
            InitializeComponent();
        }

        void OnTokenEntryCompleted(object sender, EventArgs e)
        {
            string text = ((Entry)sender).Text;

            Preferences.Default.Set(nameof(PreferenceKeys.Token), text);

            Toast.Make("Saved.").Show();
        }

        void OnEmailEntryCompleted(object sender, EventArgs e)
        {
            string text = ((Entry)sender).Text;

            Preferences.Default.Set(nameof(PreferenceKeys.Email), text);

            Toast.Make("Saved.").Show();
        }
    }
}