using Hybrid.Pages;

namespace Hybrid
{
    public partial class NewsletterPage : ContentPage
    {
        public NewsletterPage()
        {
            InitializeComponent();
        }

        public async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            if (RefreshablePageBase.Current?.NavigationManager != null)
            {
                var navigationManager = RefreshablePageBase.Current.NavigationManager;
                navigationManager.NavigateTo(navigationManager.Uri, true, true);
                RefreshView.IsRefreshing = false;
            }
        }
    }
}