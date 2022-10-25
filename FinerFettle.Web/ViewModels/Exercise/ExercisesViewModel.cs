using FinerFettle.Web.Entities.Equipment;
using FinerFettle.Web.Models;
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
        public MuscleGroups? RecoveryMuscle { get; private init; }

        [DisplayName("Sports Focus")]
        public SportsFocus? SportsFocus { get; private init; }

        [DisplayName("Muscle Contractions")]
        public MuscleContractions? MuscleContractions { get; private init; }

        [DisplayName("Exercise Type")]
        public ExerciseType? ExerciseType { get; private init; }

        [DisplayName("Show Filtered Out")]
        public bool ShowFilteredOut { get; private init; } = false;

        [DisplayName("Only Weighted Exercises")]
        public NoYes? OnlyWeights { get; private init; }

        [DisplayName("Only Core Exercises")]
        public NoYes? OnlyCore { get; private init; }

        public int? EquipmentBinder { get; set; }

        public bool FormHasData => SportsFocus.HasValue 
            || RecoveryMuscle.HasValue 
            || ExerciseType.HasValue
            || OnlyWeights.HasValue
            || EquipmentBinder.HasValue
            || MuscleContractions.HasValue;

        [DisplayName("Equipment")]
        public IList<Equipment> Equipment { get; set; } = new List<Equipment>();
    }
}
