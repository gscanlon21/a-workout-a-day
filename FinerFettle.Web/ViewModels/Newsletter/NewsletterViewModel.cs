using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Exercise;
using FinerFettle.Web.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class NewsletterViewModel : ExercisesViewModel
    {
        /// <summary>
        /// The number of footnotes to show in the newsletter
        /// </summary>
        public readonly int FootnoteCount = 3;

        public NewsletterViewModel(IList<ExerciseViewModel> exercises, Models.User.User user, Models.Newsletter.Newsletter newsletter, string token)
            : base(exercises, user.EmailVerbosity)
        {
            User = new UserNewsletterViewModel(user, token);
            Newsletter = newsletter;
        }

        public UserNewsletterViewModel User { get; }
        public Models.Newsletter.Newsletter? Newsletter { get; }

        /// <summary>
        /// Show/hide content that should only be visible in the demo?
        /// </summary>
        public bool? Demo { get; init; }

        public IList<ExerciseViewModel>? RecoveryExercises { get; set; }
        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel>? SportsExercises { get; set; }
        public IList<ExerciseViewModel>? CooldownExercises { get; set; }

        /// <summary>
        /// What exercise type is the workout today targeting?
        /// </summary>
        public ExerciseType ExerciseType { get; set; }

        /// <summary>
        /// What muscle groups is the workout today targeting?
        /// </summary>
        public MuscleGroups MuscleGroups { get; init; }

        /// <summary>
        /// Display which equipment the user does not have.
        /// </summary>
        [UIHint(nameof(Equipment))]
        public EquipmentViewModel AllEquipment { get; init; } = null!;
    }
}
