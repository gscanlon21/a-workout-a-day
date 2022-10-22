using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    [DebuggerDisplay("{Variation,nq}: {ActivityLevel}, {IntensityLevel}")]
    public class ExerciseViewModel
    {
        public ExerciseViewModel(ExerciseQueryBuilder.QueryResults result, ExerciseActivityLevel activityLevel)
        {
            Exercise = result.Exercise;
            Variation = result.Variation;
            ExerciseVariation = result.ExerciseVariation;
            IntensityLevel = result.IntensityLevel ?? (IntensityLevel?)result.User?.StrengtheningPreference;
            ActivityLevel = activityLevel;

            if (result.User != null)
            {
                Verbosity = result.User.EmailVerbosity;
            }
            else
            {
                Verbosity = Verbosity.Debug;
            }
        }

        public ExerciseViewModel(ExerciseQueryBuilder.QueryResults result, ExerciseActivityLevel activityLevel, string token) : this(result, activityLevel)
        {
            User = result.User != null ? new User.UserNewsletterViewModel(result.User, token) : null;
        }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise?
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; init; }

        public IntensityLevel? IntensityLevel { get; init; }

        public Models.Exercise.Exercise Exercise { get; init; } = null!;

        public Variation Variation { get; init; } = null!;

        public ExerciseVariation ExerciseVariation { get; init; } = null!;

        public User.UserNewsletterViewModel? User { get; init; }

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
        public Verbosity Verbosity { get; set; } = Verbosity.Normal;

        /// <summary>
        /// Should hide detail not shown in the landing page demo?
        /// </summary>
        public bool Demo => User != null && User.Email == Models.User.User.DemoUser;
    }
}
