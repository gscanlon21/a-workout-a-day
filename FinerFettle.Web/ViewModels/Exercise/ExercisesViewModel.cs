using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.ViewModels.Newsletter;
using System.ComponentModel;

namespace FinerFettle.Web.ViewModels.Exercise
{
    public class ExercisesViewModel
    {
        public ExercisesViewModel() { }

        public IList<ExerciseViewModel> Exercises { get; set; } = null!;

        /// <summary>
        /// How much detail to show in the newsletter.
        /// </summary>
        public Verbosity Verbosity => Verbosity.Debug;

        /// <summary>
        /// Don't strengthen this muscle group, but do show recovery variations for exercises
        /// </summary>
        [DisplayName("Recovery Muscle")]
        public MuscleGroups? RecoveryMuscle { get; set; }

        /// <summary>
        /// Include a section to boost a specific sports performance
        /// </summary>
        [DisplayName("Sports Focus")]
        public SportsFocus? SportsFocus { get; set; }

        [DisplayName("Show Filtered Out")]
        public bool ShowFilteredOut { get; set; } = false;

        public bool FormHasData => SportsFocus.HasValue || RecoveryMuscle.HasValue;
    }
}
