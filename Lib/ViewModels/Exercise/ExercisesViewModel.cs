using Core.Models;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Lib.ViewModels.Exercise;

/// <summary>
/// Viewmodel for All.cshtml
/// </summary>
public class ExercisesViewModel
{
    public ExercisesViewModel() { }

    public IList<Newsletter.ExerciseViewModel> Exercises { get; set; } = null!;

    public static Verbosity Verbosity => Verbosity.Debug;

    [Display(Name = "Mobility Joints")]
    public Joints? Joints { get; init; }

    [Display(Name = "Sports Focus")]
    public SportsFocus? SportsFocus { get; init; }

    [Display(Name = "Strength Muscle")]
    public MuscleGroups? StrengthMuscle { get; init; }

    [Display(Name = "Secondary Muscle")]
    public MuscleGroups? SecondaryMuscle { get; init; }

    [Display(Name = "Stretch Muscle")]
    public MuscleGroups? StretchMuscle { get; init; }

    [Display(Name = "Movement Patterns")]
    public MovementPattern? MovementPatterns { get; init; }

    [Display(Name = "Muscle Contractions")]
    public MuscleContractions? MuscleContractions { get; init; }

    [Display(Name = "Muscle Movement")]
    public MuscleMovement? MuscleMovement { get; init; }

    [Display(Name = "Exercise Focus")]
    public ExerciseFocus? ExerciseFocus { get; init; }

    [Display(Name = "Exercise Type")]
    public ExerciseType? ExerciseType { get; init; }

    [Display(Name = "Invert Filters")]
    public bool InvertFilters { get; init; } = false;

    [Display(Name = "Show Static Images")]
    public bool ShowStaticImages { get; init; } = false;

    [Display(Name = "Only Weighted Exercises")]
    public NoYes? OnlyWeights { get; init; }

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
        ExerciseFocus.HasValue
        || ExerciseType.HasValue
        || OnlyWeights.HasValue
        || EquipmentBinder.HasValue
        || StrengthMuscle.HasValue
        || SecondaryMuscle.HasValue
        || StretchMuscle.HasValue
        || MovementPatterns.HasValue
        || MuscleMovement.HasValue
        || Joints.HasValue
        || SportsFocus.HasValue
        || MuscleContractions.HasValue;

    [Display(Name = "Equipment")]
    public IList<Equipment.EquipmentViewModel> Equipment { get; set; } = new List<Equipment.EquipmentViewModel>();
}
