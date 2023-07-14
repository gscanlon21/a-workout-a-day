using Hybrid.Pages;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Hybrid;

public partial class NewsletterPage : ContentPage
{
    public NewsletterPage()
    {
        InitializeComponent();

        var viewModel = new NewsletterPageViewModel();
        BindingContext = viewModel;
        viewModel.Init(rootComponent);
    }

    public NewsletterPage(DateOnly date)
    {
        InitializeComponent();

        var viewModel = new NewsletterPageViewModel(date);
        BindingContext = viewModel;
        viewModel.Init(rootComponent);
    }

    public async void RefreshView_Refreshing(object sender, EventArgs e)
    {
        if (RefreshablePageBase.Current?.NavigationManager != null)
        {
            var navigationManager = RefreshablePageBase.Current.NavigationManager;
            navigationManager.NavigateTo(navigationManager.Uri, true, true);
            RefreshView.IsRefreshing = false;
        }
    }
}

//[QueryProperty(nameof(Date), nameof(Date))] if shell navigation
public class NewsletterPageViewModel
{
    public Dictionary<string, object?> Parameters { get; set; }

    public NewsletterPageViewModel()
    {
        Parameters = new Dictionary<string, object?>
        {
            { "Email", Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null) },
            { "Token", Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null) },
        };
    }

    public NewsletterPageViewModel(DateOnly date)
    {
        Parameters = new Dictionary<string, object?>
        {
            { "Date", date },
            { "Email", Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null) },
            { "Token", Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null) },
        };
    }

    /// <summary>
    /// Cannot for the life of me get the Parameters attribute in xaml to accept the Parameters dictionary.
    /// </summary>
    public void Init(RootComponent rootComponent)
    {
        rootComponent.Parameters = Parameters;
    }
}