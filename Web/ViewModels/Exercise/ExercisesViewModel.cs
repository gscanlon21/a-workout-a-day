using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Exercise;

/// <summary>
/// Viewmodel for All.cshtml
/// </summary>
public class ExercisesViewModel
{
    public ExercisesViewModel() { }

    public IList<Lib.ViewModels.Newsletter.ExerciseVariationViewModel> Exercises { get; set; } = null!;

    public Verbosity Verbosity => Verbosity.Debug;

    [Display(Name = "Exercise Name")]
    public string? Name { get; init; }

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

    [Display(Name = "Section")]
    public Section? Section { get; init; }

    [Display(Name = "Equipment")]
    public Equipment? Equipment { get; init; }

    public bool FormHasData =>
        !string.IsNullOrWhiteSpace(Name)
        || ExerciseFocus.HasValue
        || Section.HasValue
        || Equipment.HasValue
        || StrengthMuscle.HasValue
        || SecondaryMuscle.HasValue
        || StretchMuscle.HasValue
        || MovementPatterns.HasValue
        || MuscleMovement.HasValue
        || Joints.HasValue
        || SportsFocus.HasValue
        || MuscleContractions.HasValue;
}
