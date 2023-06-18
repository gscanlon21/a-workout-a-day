using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Native.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public MainViewModel()
    {
        var dateRange = Enumerable.Range(0, 30);

        Dates = new ObservableCollection<DateOnly>(dateRange.Select(dr => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1 * dr)));
    }


    private ObservableCollection<DateOnly> _dates;

    public ObservableCollection<DateOnly> Dates
    {
        get => _dates;
        set
        {
            if (_dates != value)
            {
                _dates = value;
                OnPropertyChanged(); // reports this property
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
