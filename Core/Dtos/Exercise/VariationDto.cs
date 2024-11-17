using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.Exercise;

// TODO: Implement IValidatableObject and setup model validation instead of using the /exercises/check route
/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class VariationDto
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The filename.ext of the static content image
    /// </summary>
    [Required]
    public string StaticImage { get; init; } = null!;

    /// <summary>
    /// The filename.ext of the animated content image
    /// </summary>
    public string? AnimatedImage { get; set; }

    /// <summary>
    /// Does this variation work one side at a time or both sides at once?
    /// </summary>
    [Required]
    public bool Unilateral { get; set; }

    /// <summary>
    /// Is this variation dangerous and needs to be exercised with caution?
    /// </summary>
    [Required]
    public bool UseCaution { get; set; }

    /// <summary>
    /// Can the variation be performed with weights?
    /// 
    /// This controls whether the Pounds selector shows to the user.
    /// </summary>
    [Required]
    public bool IsWeighted { get; set; }

    /// <summary>
    /// Count reps or time?
    /// </summary>
    public bool? PauseReps { get; set; }

    /// <summary>
    /// Does this variation work muscles by moving weights or holding them in place?
    /// </summary>
    [Required]
    public MuscleMovement MuscleMovement { get; init; }

    /// <summary>
    /// What functional movement patterns does this variation work?
    /// </summary>
    [Required]
    public MovementPattern MovementPattern { get; init; }

    /// <summary>
    /// Primary muscles strengthened by the exercise
    /// </summary>
    [Required]
    public MusculoskeletalSystem Strengthens { get; init; }

    /// <summary>
    /// Primary muscles stretched by the exercise
    /// </summary>
    [Required]
    public MusculoskeletalSystem Stretches { get; init; }

    /// <summary>
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    [Required]
    public MusculoskeletalSystem Stabilizes { get; init; }

    /// <summary>
    /// What is this variation focusing on?
    /// </summary>
    [Required]
    [Display(Name = "Exercise Focus", ShortName = "Focus")]
    public ExerciseFocus ExerciseFocus { get; init; }

    public virtual int ExerciseId { get; init; }

    [JsonIgnore]
    public virtual ExerciseDto Exercise { get; init; } = null!;

    /// <summary>
    /// The progression range required to view the exercise variation.
    /// </summary>
    [Required]
    public ProgressionDto Progression { get; init; } = null!;

    /// <summary>
    /// What type of exercise is this variation?
    /// </summary>
    [Required]
    [Display(Name = "Section")]
    public Section Section { get; init; }

    /// <summary>
    /// What sports does performing this exercise benefit.
    /// </summary>
    [Required]
    [Display(Name = "Sports Focus", ShortName = "Sports")]
    public SportsFocus SportsFocus { get; init; }

    public string? DisabledReason { get; init; } = null;

    /// <summary>
    /// Notes about the variation (externally shown)
    /// </summary>
    public string? Notes { get; init; } = null;

    /// <summary>
    /// Combination of this variations Strength, Stretch and Stability muscles worked.
    /// </summary>
    [NotMapped]
    public MusculoskeletalSystem AllMuscles => Strengthens | Stretches | Stabilizes;

    public string? DefaultInstruction { get; init; }

    // Cannot have an InverseProperty because we have two navigation properties to Instruction
    [UIHint(nameof(InstructionDto))] //[JsonIgnore, InverseProperty(nameof(Instruction.Variation))]
    public virtual ICollection<InstructionDto> Instructions { get; init; } = [];

    [JsonIgnore]
    public virtual ICollection<UserVariationDto> UserVariations { get; init; } = null!;

    [JsonIgnore]
    public virtual ICollection<UserWorkoutVariation> UserWorkoutVariations { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is VariationDto other
        && other.Id == Id;

    public bool HasRootInstructions => Instructions.Any();

    public IOrderedEnumerable<InstructionDto> GetRootInstructions(UserNewsletterDto? user)
    {
        return Instructions
            // Only show the optional equipment groups that the user owns the equipment of.
            .Where(eg => user == null
                // Or the instruction doesn't have any equipment.
                || eg.Equipment == Core.Models.Equipment.Equipment.None
                // Or the user owns the equipment of the root instruction.
                || (user.Equipment & eg.Equipment) != 0
                    // And the root instruction can be done on its own, or is an ordered difficulty.
                    // Or the user owns the equipment of the child instructions.
                    && (eg.Link != null || eg.Order != null || eg.GetChildInstructions(user).Any()))
            // Keep the order consistent across newsletters
            .OrderByDescending(eg => eg.HasChildInstructions && !eg.Order.HasValue)
            .ThenBy(eg => eg.Order ?? int.MaxValue)
            .ThenBy(eg => eg.Name)
            .ThenBy(eg => eg.Id);
    }
}

/// <summary>
/// The range of progressions an exercise is available for.
/// </summary>
public record ProgressionDto([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
{
    public int MinOrDefault => Min ?? 0;
    public int MaxOrDefault => Max ?? 100;
}
