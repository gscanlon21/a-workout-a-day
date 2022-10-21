using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    [DebuggerDisplay("{Variation,nq}: {ActivityLevel}, {IntensityLevel}")]
    public class ExerciseViewModel
    {
        public ExerciseViewModel(User.UserNewsletterViewModel? user, Variation variation, ExerciseProgression exerciseProgression, IntensityLevel? intensityLevel, ExerciseActivityLevel activityLevel)
        {
            User = user;
            Exercise = exerciseProgression.Exercise;
            Variation = variation;
            ExerciseProgression = exerciseProgression;
            IntensityLevel = intensityLevel ?? (IntensityLevel?)user?.StrengtheningPreference;
            ActivityLevel = activityLevel;

            if (user != null)
            {
                Verbosity = user.EmailVerbosity;
            }
            else
            {
                Verbosity = Verbosity.Debug;
            }
        }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise?
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; }

        public IntensityLevel? IntensityLevel { get; }

        public Models.Exercise.Exercise Exercise { get; }

        public Variation Variation { get; }

        public ExerciseProgression ExerciseProgression { get; }

        public User.UserNewsletterViewModel? User { get; }

        public Models.User.UserExercise? UserExercise { get; set; }
        
        public bool HasLowerProgressionVariation { get; set; }
        public bool HasHigherProgressionVariation { get; set; }

        [UIHint("Proficiency")]
        public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
            .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
            .OrderBy(intensity => intensity.IntensityLevel)
            .Select(intensity => new ProficiencyViewModel(intensity) { ShowName = IntensityLevel == null })
            .ToList();

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
