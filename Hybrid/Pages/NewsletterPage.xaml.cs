using Hybrid.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Hybrid;

public partial class NewsletterPage : ContentPage
{
    private readonly string RefreshId = Guid.NewGuid().ToString();

    public NewsletterPage()
    {
        InitializeComponent();

        var viewModel = new NewsletterPageViewModel(RefreshId);
        BindingContext = viewModel;
        viewModel.Init(rootComponent);
    }

    public NewsletterPage(DateOnly date)
    {
        InitializeComponent();

        var viewModel = new NewsletterPageViewModel(RefreshId, date);
        BindingContext = viewModel;
        viewModel.Init(rootComponent);
    }

    public async void RefreshView_Refreshing(object sender, EventArgs e)
    {
        if (RefreshablePageBase.Current.TryGetValue(RefreshId, out NavigationManager? navManager) && navManager != null)
        {
            navManager.NavigateTo(navManager.Uri, true, true);
            RefreshView.IsRefreshing = false;
        }
    }
}

//[QueryProperty(nameof(Date), nameof(Date))] if shell navigation
public class NewsletterPageViewModel
{
    public Dictionary<string, object?> Parameters { get; set; }

    public NewsletterPageViewModel(string refreshId)
    {
        Parameters = new Dictionary<string, object?>
        {
            { nameof(RefreshableLibMain.Email), Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null) },
            { nameof(RefreshableLibMain.Token), Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null) },
            { nameof(RefreshablePageBase.RefreshId), refreshId },
        };
    }

    public NewsletterPageViewModel(string refreshId, DateOnly date)
    {
        Parameters = new Dictionary<string, object?>
        {
            { nameof(RefreshableLibMain.Date), date },
            { nameof(RefreshableLibMain.Email), Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null) },
            { nameof(RefreshableLibMain.Token), Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null) },
            { nameof(RefreshablePageBase.RefreshId), refreshId },
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