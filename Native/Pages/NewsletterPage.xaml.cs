using CommunityToolkit.Maui.Alerts;
using Native.Core;
using Native.Models;
using Native.Services;
using Native.ViewModels;
using System.Collections.ObjectModel;

namespace Native
{
    public partial class NewsletterPage : ContentPage
    {
        public NewsletterModel? Newsletter { get; set; }

        public ObservableCollection<ExerciseModel> Exercises { get; set; }

        public NewsletterPage(RestService restService)
        {
            InitializeComponent();

            BindingContext = new NewsletterViewModel(restService);
            _ = LoadMyData(restService);
        }

        void OnEntryCompleted(object sender, EventArgs e)
        {
            string text = ((Entry)sender).Text;

            Preferences.Default.Set(nameof(PreferenceKeys.Token), text);

            Toast.Make("Saved.").Show();
        }

        public async Task LoadMyData(RestService restService)
        {
            try
            {
                // Hide people display
                // Show loading indicator
                //Newsletter = (await restService.RefreshDataAsync())!;
                //Exercises = new ObservableCollection<ExerciseViewModel>(Newsletter.MainExercises.ToList());
                // Show people display
            }
            catch (Exception)
            {
                // Show error indicator
            }
            finally
            {
                // Hide loading indicator
            }
        }
    }
}