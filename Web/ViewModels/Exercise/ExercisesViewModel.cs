using System.ComponentModel.DataAnnotations;
using Web.Entities.Equipment;
using Web.Models;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;

namespace Web.ViewModels.Exercise;

/// <summary>
/// Viewmodel for All.cshtml
/// </summary>
public class ExercisesViewModel
{
    public ExercisesViewModel() { }

    public IList<ExerciseViewModel> Exercises { get; set; } = null!;

    public Verbosity Verbosity => Verbosity.Debug;

    [Display(Name = "Recovery Muscle")]
    public MuscleGroups? RecoveryMuscle { get; init; }

    [Display(Name = "Sports Focus")]
    public SportsFocus? SportsFocus { get; init; }

    [Display(Name = "Strength Muscle")]
    public MuscleGroups? StrengthMuscle { get; init; }

    [Display(Name = "Stability Muscle")]
    public MuscleGroups? StabilityMuscle { get; init; }

    [Display(Name = "Stretch Muscle")]
    public MuscleGroups? StretchMuscle { get; init; }

    [Display(Name = "Movement Patterns")]
    public MovementPattern? MovementPatterns { get; init; }

    [Display(Name = "Muscle Contractions")]
    public MuscleContractions? MuscleContractions { get; init; }

    [Display(Name = "Muscle Movement")]
    public MuscleMovement? MuscleMovement { get; init; }

    [Display(Name = "Exercise Type")]
    public ExerciseType? ExerciseType { get; init; }

    [Display(Name = "Exercise Section")]
    public ExerciseSection? ExerciseSection { get; init; }

    [Display(Name = "Invert Filters")]
    public bool InvertFilters { get; init; } = false;

    [Display(Name = "Show Static Images")]
    public bool ShowStaticImages { get; init; } = false;

    [Display(Name = "Only Weighted Exercises")]
    public NoYes? OnlyWeights { get; init; }

    [Display(Name = "Only Anti-Gravity Exercises")]
    public NoYes? OnlyAntiGravity { get; init; }

    [Display(Name = "Only Unilateral Exercises")]
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

    public bool FormHasData =>
        ExerciseSection.HasValue
        || ExerciseType.HasValue
        || OnlyWeights.HasValue
        || OnlyAntiGravity.HasValue
        || EquipmentBinder.HasValue
        || StrengthMuscle.HasValue
        || StabilityMuscle.HasValue
        || StretchMuscle.HasValue
        || MovementPatterns.HasValue
        || OnlyUnilateral.HasValue
        || MuscleMovement.HasValue
        || RecoveryMuscle.HasValue
        || SportsFocus.HasValue
        || MuscleContractions.HasValue;

    [Display(Name = "Equipment")]
    public IList<Equipment> Equipment { get; set; } = new List<Equipment>();
}
