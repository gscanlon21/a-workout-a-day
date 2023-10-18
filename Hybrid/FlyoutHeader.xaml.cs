using System.Diagnostics.CodeAnalysis;

namespace Hybrid;

public partial class FlyoutHeader : ContentView
{
    public FlyoutHeader()
    {
        InitializeComponent();

        BindingContext = this;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Xaml cannot bind to static string")]
    public string CurrentDate => DateTime.UtcNow.ToLongDateString();

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Xaml cannot bind to static string")]
    public string? Email => Preferences.Default.Get<string?>(nameof(PreferenceKeys.Email), null);
}