using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attribute
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Models.User.User? user, Exercise exercise, Variation variation, IntensityLevel? intensityLevel)
        {
            User = user;
            Exercise = exercise;
            Variation = variation;
            IntensityLevel = intensityLevel ?? (IntensityLevel?)user?.StrengtheningPreference;

            if (user != null)
            {
                Verbosity = user.EmailVerbosity;
            }
        }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise?
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; init; }

        public Models.User.User? User { get; init; }

        public Exercise Exercise { get; init; }
        public Variation Variation { get; init; }

        public IntensityLevel? IntensityLevel { get; set; }

        [UIHint("Proficiency")]
        public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
            .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
            .Select(intensity => new ProficiencyViewModel(intensity) { ShowName = IntensityLevel == null }).ToList();

        public Models.User.UserExercise? UserExercise { get; set; }

        public bool HasLowerProgressionVariation { get; set; }
        public bool HasHigherProgressionVariation { get; set; }

        /// <summary>
        /// How much detail to show of the exercise?
        /// </summary>
        public Verbosity Verbosity { get; init; } = Verbosity.Normal;

        /// <summary>
        /// Should hide detail not shown in the landing page demo?
        /// </summary>
        public bool Demo { get; init; } = false;
    }
}
