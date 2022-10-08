using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attribute
    public class ExerciseViewModel
    {
        public ExerciseViewModel(ExerciseViewModel copy)
        {
            User = copy.User;
            Exercise = copy.Exercise;
            Variation = copy.Variation;
            Intensity = copy.Intensity;
            Verbosity = copy.Verbosity;
            IntensityPreference = copy.IntensityPreference;
            ActivityLevel = copy.ActivityLevel;
            Demo = copy.Demo;
            UserProgression = copy.UserProgression;
        }

        public ExerciseViewModel(Models.User.User? user, Exercise exercise, Variation variation, Intensity intensity, IntensityLevel? intensityLevel)
        {
            User = user;
            Exercise = exercise;
            Variation = variation;
            Intensity = intensity;
            IntensityPreference = new ProficiencyViewModel(Intensity, intensityLevel ?? (IntensityLevel?)user?.StrengtheningPreference);

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

        public Intensity Intensity { get; init; }

        [UIHint(nameof(Intensity))]
        public ProficiencyViewModel IntensityPreference { get; set; }

        public Models.User.ExerciseUserProgression? UserProgression { get; set; }

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
