using System.Diagnostics.CodeAnalysis;

namespace Hybrid;

public partial class FlyoutFooter : ContentView
{
    public FlyoutFooter()
    {
        InitializeComponent();

        BindingContext = this;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Xaml cannot bind to static string")]
    public string CurrentDate => DateTime.UtcNow.ToLongDateString();
}