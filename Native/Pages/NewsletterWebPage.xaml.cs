using CommunityToolkit.Maui.Alerts;
using Native.Core;
using Native.Models;
using Native.Services;
using Native.ViewModels;
using System.Collections.ObjectModel;

namespace Native
{
    public partial class NewsletterWebPage : ContentPage
    {
        public string Source { get; set; }

        public NewsletterWebPage()
        {
            InitializeComponent();

            var email = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null);
            var token = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null);

            Source = $"https://aworkoutaday.com/n/{email}?token={token}";
        }
    }
}