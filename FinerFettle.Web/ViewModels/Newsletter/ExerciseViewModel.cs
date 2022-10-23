using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    [DebuggerDisplay("{Variation,nq}: {ActivityLevel}, {IntensityLevel}")]
    public class ExerciseViewModel : 
        IQueryFiltersSportsFocus, 
        IQueryFiltersExerciseType, 
        IQueryFiltersIntensityLevel,
        IQueryFiltersMuscleContractions,
        IQueryFiltersRecoveryMuscle
    {
        public ExerciseViewModel(Models.User.User? user, Models.Exercise.Exercise exercise, Variation variation, ExerciseVariation exerciseVariation, IntensityLevel? intensityLevel, ExerciseActivityLevel activityLevel)
        {
            Exercise = exercise;
            Variation = variation;
            ExerciseVariation = exerciseVariation;
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

        public ExerciseViewModel(Models.User.User? user, Models.Exercise.Exercise exercise, Variation variation, ExerciseVariation exerciseVariation, IntensityLevel? intensityLevel, ExerciseActivityLevel activityLevel, string token) 
            : this(user, exercise, variation, exerciseVariation, intensityLevel, activityLevel)
        {
            User = user != null ? new User.UserNewsletterViewModel(user, token) : null;
        }

        public ExerciseViewModel(ExerciseQueryBuilder.QueryResults result, ExerciseActivityLevel activityLevel) 
            : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation, result.IntensityLevel, activityLevel) { }

        public ExerciseViewModel(ExerciseQueryBuilder.QueryResults result, ExerciseActivityLevel activityLevel, string token) 
            : this(result.User, result.Exercise, result.Variation, result.ExerciseVariation, result.IntensityLevel, activityLevel, token) { }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise? Really the theme of the exercise view.
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; set; }

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
