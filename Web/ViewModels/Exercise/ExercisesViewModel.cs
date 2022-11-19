using Web.Entities.Equipment;
using Web.Models;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Http.Extensions;
using System.ComponentModel;

namespace Web.ViewModels.Exercise;

public class ExercisesViewModel
{
    public ExercisesViewModel() { }

    public IList<ExerciseViewModel> Exercises { get; set; } = null!;

    public Verbosity Verbosity => Verbosity.Debug;

    [DisplayName("Recovery Muscle")]
    public MuscleGroups? RecoveryMuscle { get; init; } = MuscleGroups.None;

    [DisplayName("Sports Focus")]
    public SportsFocus? SportsFocus { get; init; } = Models.User.SportsFocus.None;

    [DisplayName("Include Muscle")]
    public MuscleGroups? IncludeMuscle { get; init; }

    [DisplayName("Movement Patterns")]
    public MovementPattern? MovementPatterns { get; init; }

    [DisplayName("Muscle Contractions")]
    public MuscleContractions? MuscleContractions { get; init; }

    [DisplayName("Muscle Movement")]
    public MuscleMovement? MuscleMovement { get; init; }

    [DisplayName("Exercise Type")]
    public ExerciseType? ExerciseType { get; init; }

    [DisplayName("Show Filtered Out")]
    public bool ShowFilteredOut { get; init; } = false;

    [DisplayName("Only Weighted Exercises")]
    public NoYes? OnlyWeights { get; init; }

    [DisplayName("Only Anti-Gravity Exercises")]
    public NoYes? OnlyAntiGravity { get; init; }

    [DisplayName("Only Core Exercises")]
    public NoYes? OnlyCore { get; init; }

    [DisplayName("Only Unilateral Exercises")]
    public NoYes? OnlyUnilateral { get; init; }

    public int? EquipmentBinder { get; set; }

    public IList<int>? EquipmentIds
    {
        get
        {
            if (EquipmentBinder.HasValue)
            {
                if (EquipmentBinder.Value == 0)
                {
                    return new List<int>(0);
                }
                else
                {
                    return new List<int>(1) { EquipmentBinder.Value };
                }
            }

            return null;
        }
    }

    public bool FormHasData => ExerciseType.HasValue
        || OnlyWeights.HasValue
        || OnlyCore.HasValue
        || OnlyAntiGravity.HasValue
        || EquipmentBinder.HasValue
        || IncludeMuscle.HasValue
        || MovementPatterns.HasValue
        || OnlyUnilateral.HasValue
        || MuscleMovement.HasValue
        || MuscleContractions.HasValue;

    [DisplayName("Equipment")]
    public IList<Equipment> Equipment { get; set; } = new List<Equipment>();
}
