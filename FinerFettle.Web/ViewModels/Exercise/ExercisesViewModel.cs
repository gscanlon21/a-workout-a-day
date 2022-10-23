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

        public Verbosity Verbosity => Verbosity.Debug;

        [DisplayName("Recovery Muscle")]
        public MuscleGroups? RecoveryMuscle { get; set; }

        [DisplayName("Sports Focus")]
        public SportsFocus? SportsFocus { get; set; }

        [DisplayName("Muscle Contractions")]
        public MuscleContractions? MuscleContractions { get; set; }

        [DisplayName("Exercise Type")]
        public ExerciseType? ExerciseType { get; set; }

        [DisplayName("Show Filtered Out")]
        public bool ShowFilteredOut { get; set; } = false;

        public bool FormHasData => SportsFocus.HasValue 
            || RecoveryMuscle.HasValue 
            || ExerciseType.HasValue
            || MuscleContractions.HasValue;
    }
}
