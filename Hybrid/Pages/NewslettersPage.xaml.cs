using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.ViewModels.Newsletter;
using Lib.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

public class NewslettersPageViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;

    public IAsyncRelayCommand LoadCommand { get; }

    public NewslettersPageViewModel(UserService userService)
    {
        _userService = userService;

        LoadCommand = new AsyncRelayCommand(LoadWorkoutsAsync);
        NewsletterCommand = new Command<UserWorkoutViewModel>(async (UserWorkoutViewModel arg) =>
        {
            await Navigation.PushAsync(new NewsletterPage(arg.Date));
        });

        Task.Run(LoadWorkoutsAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public INavigation Navigation { get; set; } = null!;

    public ICommand NewsletterCommand { private set; get; }

    /*
    public ObservableCollection<DateOnly> Dates { get; set; } = new ObservableCollection<DateOnly>(
        Enumerable.Range(0, 8).Select(i => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-i))
    );
    */
    private IList<UserWorkoutViewModel> _workouts;
    public IList<UserWorkoutViewModel> Workouts
    {
        get => _workouts;
        set
        {
            if (value != _workouts)
            {
                _workouts = value;
                NotifyPropertyChanged();
            }
        }
    }

    private bool _isBusy = true;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (value != _isBusy)
            {
                _isBusy = value;
                NotifyPropertyChanged();
            }
        }
    }

    private async Task LoadWorkoutsAsync()
    {
        var email = Preferences.Default.Get(nameof(PreferenceKeys.Email), "");
        var token = Preferences.Default.Get(nameof(PreferenceKeys.Token), "");
        Workouts = await _userService.GetWorkouts(email, token);
        IsBusy = false;
    }
}