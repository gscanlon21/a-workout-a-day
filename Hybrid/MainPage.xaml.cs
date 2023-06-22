namespace Hybrid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            //BindingContext = new MainViewModel(); // BindingContext = this;
        }

        /*
        private void OnLoginClicked(object sender, EventArgs e)
        {
            _ = Shell.Current.GoToAsync(nameof(LoginPage));
            //Navigation.PushAsync(new LoginPage());
        }
        */
    }
}