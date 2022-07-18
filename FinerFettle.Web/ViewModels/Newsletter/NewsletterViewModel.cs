using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class NewsletterViewModel
    {
        public NewsletterViewModel() : this(null) { }
        public NewsletterViewModel(IList<ExerciseViewModel>? exercises)
        {
            Exercises = exercises;
        }

        public User? User { get; set; }
        public IList<ExerciseViewModel>? Exercises { get; init; }
    }
}
