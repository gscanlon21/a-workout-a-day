using Native.Core;
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

class NewsletterWebViewModel : INotifyPropertyChanged
{
    public NewsletterWebViewModel()
    {
        var email = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null);
        var token = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null);

        Source = $"https://aworkoutaday.com/n/{email}?token={token}";
    }

    private string _source;

    public string Source
    {
        get => _source;
        set
        {
            if (_source != value)
            {
                _source = value;
                OnPropertyChanged(); // reports this property
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}