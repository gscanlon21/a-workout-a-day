using Native.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Native.ViewModels;

class NewsletterWebViewModel : IQueryAttributable, INotifyPropertyChanged
{
    public NewsletterWebViewModel() { }

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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var email = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null);
        var token = Preferences.Default.Get<string?>(nameof(PreferenceKeys.Token), null);

        var dateParam = (query["Date"] as DateOnly?) ?? DateOnly.FromDateTime(DateTime.UtcNow);

        Source = $"https://aworkoutaday.com/n/{email}/{dateParam:O}?token={token}";

        Debug.WriteLine(Source);

        OnPropertyChanged();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}