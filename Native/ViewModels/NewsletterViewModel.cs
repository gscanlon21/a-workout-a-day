using Native.Models;
using Native.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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