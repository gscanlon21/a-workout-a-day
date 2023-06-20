using Native.Models;
using Native.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Native.ViewModels;

class NewsletterViewModel : INotifyPropertyChanged
{
    public NewsletterViewModel()
    {

    }

    public NewsletterViewModel(RestService restService)
    {
        _ = LoadDataAsync(restService);
    }

    public async Task LoadDataAsync(RestService restService)
    {
        var newsletter = (await restService.RefreshDataAsync())!;
        Exercises = new ObservableCollection<ExerciseModel>(newsletter.MainExercises.ToList());
    }


    private ObservableCollection<ExerciseModel> _exercises;

    public ObservableCollection<ExerciseModel> Exercises
    {
        get => _exercises;
        set
        {
            if (_exercises != value)
            {
                _exercises = value;
                OnPropertyChanged(); // reports this property
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}