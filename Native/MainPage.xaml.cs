using Native.ViewModels;
using System.Diagnostics;

namespace Native
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainViewModel(); // BindingContext = this;
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            _ = Shell.Current.GoToAsync(nameof(LoginPage));
            //Navigation.PushAsync(new LoginPage());
        }

        /// <summary>
        /// The Selected Item Event
        /// </summary>
        async void DateSelected(object sender, SelectedItemChangedEventArgs e) // Is `async void` right?
        {
            // Get the selected Department from the ListItem
            var date = (DateOnly)e.SelectedItem;
            // Create a Navigation Parameter using the Dictionary
            var navigationParameter = new Dictionary<string, object>
            {
                { "Date",  date}
            };

            Debug.WriteLine(date);

            // Navigate to the Employees Route with the Navigation Parameter
            await Shell.Current.GoToAsync(nameof(NewsletterWebPage), navigationParameter);
        }
    }
}