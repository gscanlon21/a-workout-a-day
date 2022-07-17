using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class NewsletterViewModel
    {
        public NewsletterViewModel(Models.Newsletter.Newsletter newsletter, IList<Variation>? variations)
        {
            Type = newsletter.ExerciseType;
            Exercises = variations;
        }

        public ExerciseType Type { get; init; }
        public IList<Variation> Exercises { get; init; }
    }
}
