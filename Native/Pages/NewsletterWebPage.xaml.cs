using Native.Core;
using Native.ViewModels;
using System.Diagnostics;

namespace Native;

//[QueryProperty(nameof(Date), "Date")]
public partial class NewsletterWebPage : ContentPage
{
    public NewsletterWebPage()
    {
        InitializeComponent();

        BindingContext = new NewsletterWebViewModel();
    }

    //private DateOnly _date;
    //public DateOnly Date
    //{
    //    get => _date;
    //    set
    //    {
    //        _date = value;
    //        OnPropertyChanged();
    //    }
    //}
}