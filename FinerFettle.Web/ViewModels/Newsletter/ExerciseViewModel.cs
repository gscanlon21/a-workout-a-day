using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attributes
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Variation exercise, Intensity intensity, MuscleGroups muscles, ExerciseType type)
        {
            Exercise = exercise;
            Intensity = intensity;
            Muscles = muscles;
            ExerciseType = type;
        }

        public MuscleGroups Muscles { get; init; }

        public ExerciseType ExerciseType { get; init; }

        public Variation Exercise { get; init; }

        [UIHint(nameof(Intensity))]
        public Intensity Intensity { get; init; }

        public IList<Equipment> Equipment => Exercise.EquipmentGroups.SelectMany(e => e.Equipment).ToList();
    }
}
