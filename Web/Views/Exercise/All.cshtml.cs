using Core.Dtos.Newsletter;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Web.Views.Exercise;

public class ExercisesViewModel : IValidatableObject
{
    [ValidateNever]
    public IList<ExerciseVariationDto> Exercises { get; set; } = null!;

    [ValidateNever]
    public bool FormOpen => Exercises?.Any() != true;

    [ValidateNever]
    public Verbosity Verbosity => Verbosity.Debug;

    [Display(Name = "Name")]
    public string? Name { get; init; }

    [Display(Name = "Section")]
    public Section? Section { get; init; }

    [Display(Name = "Equipment")]
    public Equipment? Equipment { get; init; }

    [Display(Name = "Sports Focus")]
    public SportsFocus? SportsFocus { get; init; }

    [Display(Name = "Strengthens")]
    public MusculoskeletalSystem? StrengthMuscle { get; init; }

    [Display(Name = "Stabilizes")]
    public MusculoskeletalSystem? SecondaryMuscle { get; init; }

    [Display(Name = "Stretches")]
    public MusculoskeletalSystem? StretchMuscle { get; init; }

    [Display(Name = "Movement Patterns")]
    public MovementPattern? MovementPatterns { get; init; }

    [Display(Name = "Muscle Movement")]
    public MuscleMovement? MuscleMovement { get; init; }

    [Display(Name = "Exercise Focus")]
    public ExerciseFocus? ExerciseFocus { get; init; }

    [Display(Name = "Visual Skills")]
    public int? VisualSkills { get; init; }

    [Display(Name = "Cervical Skills")]
    public int? CervicalSkills { get; init; }

    [Display(Name = "Thoracic Skills")]
    public int? ThoracicSkills { get; init; }

    [ValidateNever]
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
        || VisualSkills.HasValue
        || CervicalSkills.HasValue
        || ThoracicSkills.HasValue
        || SportsFocus.HasValue;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        List<MusculoskeletalSystem?> allMuscles = [StrengthMuscle, StretchMuscle, SecondaryMuscle];
        if (allMuscles.Count(mg => mg.HasValue) > 1)
        {
            yield return new ValidationResult("Only one muscle group may be selected.");
        }

        List<int?> allSkills = [VisualSkills, CervicalSkills, ThoracicSkills];
        if (allSkills.Count(mg => mg.HasValue) > 1)
        {
            yield return new ValidationResult("Only one skill group may be selected.");
        }
    }
}
