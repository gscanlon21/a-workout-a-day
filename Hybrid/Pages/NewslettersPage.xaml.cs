using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Hybrid
{
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

    public class NewslettersPageViewModel
    {
        public ObservableCollection<DateOnly> Dates { get; set; } = new ObservableCollection<DateOnly>(
            Enumerable.Range(0, 7).Select(i => DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-i)))
        );

        public INavigation Navigation { get; set; } = null!;

        [Inject]
        public IServiceProvider ServiceProvider { get; set; } = null!;

        public NewslettersPageViewModel() 
        {
            NewsletterCommand = new Command<DateOnly>(async (DateOnly arg) =>
            {
                await Navigation.PushModalAsync(new NewsletterPage(arg));
            });
        }

        public ICommand NewsletterCommand { private set; get; }
    }
}