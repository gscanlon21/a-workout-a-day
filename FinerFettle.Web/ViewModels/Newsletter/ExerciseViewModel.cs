using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attribute
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Models.User.User? user, Exercise exercise, Variation variation, Intensity intensity)
        {
            User = user;
            Exercise = exercise;
            Variation = variation;
            Intensity = intensity;

            if (user != null)
            {
                Verbosity = user.EmailVerbosity;
            }

            if (intensity.IntensityLevel == IntensityLevel.Main)
            {
                ActivityLevel = ExerciseActivityLevel.Main;
            } 
            else if (intensity.IntensityLevel == IntensityLevel.Stretch)
            {
                if (variation.MuscleContractions == MuscleContractions.Isometric)
                {
                    // Choose static stretches for cooldown exercises
                    ActivityLevel = ExerciseActivityLevel.Cooldown;
                }
                else
                {
                    // Choose dynamic stretches for warmup exercises.
                    // Warmup exercises may include short isometric holds between reps.
                    ActivityLevel = ExerciseActivityLevel.Warmup;
                }
            }
        }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise?
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; }

        public Models.User.User? User { get; init; }

        public Exercise Exercise { get; init; }
        public Variation Variation { get; init; }

        [UIHint(nameof(Intensity))]
        public Intensity Intensity { get; init; }

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
