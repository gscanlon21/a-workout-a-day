namespace Native
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCurrentNewsletterClicked(object sender, EventArgs e)
        {
            _ = Shell.Current.GoToAsync(nameof(NewsletterPage));
        }

        private void OnCurrentWebNewsletterClicked(object sender, EventArgs e)
        {
            _ = Shell.Current.GoToAsync(nameof(NewsletterWebPage));
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LoginPage());
        }
    }
}