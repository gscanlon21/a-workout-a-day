using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Dtos.Newsletter;
using Hybrid.Database;
using Lib.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Hybrid;

public partial class NewslettersPage : ContentPage
{
    /// https://stackoverflow.com/questions/73710578/net-maui-mvvm-navigate-and-pass-object-between-views
    public NewslettersPage(NewslettersPageViewModel viewModel)
    {
        InitializeComponent();
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }
}

public partial class NewslettersPageViewModel : ObservableObject
{
    private readonly NewsletterService _newsletterService;
    private readonly UserPreferences _preferences;

    public INavigation Navigation { get; set; } = null!;

    public ICommand NewsletterCommand { get; }

    public IAsyncRelayCommand LoadCommand { get; }

    public NewslettersPageViewModel(NewsletterService newsletterService, UserPreferences preferences)
    {
        _newsletterService = newsletterService;
        _preferences = preferences;

        LoadCommand = new AsyncRelayCommand(LoadNewslettersAsync);
        NewsletterCommand = new Command<UserWorkoutDto>(async (UserWorkoutDto arg) =>
        {
            await Navigation.PushAsync(new NewsletterPage(arg.Date));
        });
    }

    [ObservableProperty]
    private bool _loading = true;

    [ObservableProperty]
    public ObservableCollection<UserWorkoutDto>? _newsletters = null;

    private async Task LoadNewslettersAsync()
    {
        var newsletters = await _newsletterService.GetNewsletters(_preferences.Email.Value, _preferences.Token.Value);
        Newsletters = new ObservableCollection<UserWorkoutDto>(newsletters.GetValueOrDefault([]));

        Loading = false;
    }
}