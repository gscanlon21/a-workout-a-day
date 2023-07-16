namespace Hybrid;

public partial class FlyoutFooter : ContentView
{
    public FlyoutFooter()
    {
        InitializeComponent();

        BindingContext = this;
    }

    public static string CurrentDate => DateTime.UtcNow.ToLongDateString();
}