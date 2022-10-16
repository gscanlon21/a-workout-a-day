using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    [DebuggerDisplay("ActivityLevel = {ActivityLevel}, IntensityLevel = {IntensityLevel}")]
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Models.User.User? user, Variation variation, IntensityLevel? intensityLevel, ExerciseActivityLevel activityLevel)
        {
            User = user;
            Exercise = variation.Exercise;
            Variation = variation;
            IntensityLevel = intensityLevel ?? (IntensityLevel?)user?.StrengtheningPreference;
            ActivityLevel = activityLevel;

            if (user != null)
            {
                Verbosity = user.EmailVerbosity;
            }
        }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise?
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; }

        public IntensityLevel? IntensityLevel { get; }

        public Models.Exercise.Exercise Exercise { get; }

        public Variation Variation { get; }

        public Models.User.User? User { get; }

        public Models.User.UserExercise? UserExercise { get; set; }
        
        public bool HasLowerProgressionVariation { get; set; }
        public bool HasHigherProgressionVariation { get; set; }

        [UIHint("Proficiency")]
        public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
            .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
            .Select(intensity => new ProficiencyViewModel(intensity) { ShowName = IntensityLevel == null }).ToList();

        /// <summary>
        /// How much detail to show of the exercise?
        /// </summary>
        public Verbosity Verbosity { get; init; } = Verbosity.Normal;

        /// <summary>
        /// Should hide detail not shown in the landing page demo?
        /// </summary>
        public bool? Demo { get; init; }
    }
}
