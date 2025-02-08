using Core.Dtos.User;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Core.Dtos.Exercise;

/// <summary>
/// A variation of an exercise.
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class VariationDto
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The filename.ext of the static content image.
    /// </summary>
    public string StaticImage { get; init; } = null!;

    /// <summary>
    /// The filename.ext of the animated content image.
    /// </summary>
    public string? AnimatedImage { get; init; }

    /// <summary>
    /// Does this variation work one side at a time or both sides at once?
    /// </summary>
    public bool Unilateral { get; init; }

    /// <summary>
    /// Is this variation dangerous and needs to be exercised with caution?
    /// </summary>
    public bool UseCaution { get; init; }

    /// <summary>
    /// Can the variation be performed with weights?
    /// 
    /// This controls whether the Pounds selector shows to the user.
    /// </summary>
    public bool IsWeighted { get; init; }

    /// <summary>
    /// Count reps or time?
    /// </summary>
    public bool? PauseReps { get; init; }

    /// <summary>
    /// Does this variation work muscles by moving weights or holding them in place?
    /// </summary>
    public MuscleMovement MuscleMovement { get; init; }

    /// <summary>
    /// What functional movement patterns does this variation work?
    /// </summary>
    public MovementPattern MovementPattern { get; init; }

    /// <summary>
    /// Primary muscles strengthened by the exercise
    /// </summary>
    public MusculoskeletalSystem Strengthens { get; init; }

    /// <summary>
    /// Primary muscles stretched by the exercise
    /// </summary>
    public MusculoskeletalSystem Stretches { get; init; }

    /// <summary>
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    public MusculoskeletalSystem Stabilizes { get; init; }

    /// <summary>
    /// What is this variation focusing on?
    /// </summary>
    [Display(Name = "Exercise Focus", ShortName = "Focus")]
    public ExerciseFocus ExerciseFocus { get; init; }

    /// <summary>
    /// The progression range required to view the exercise variation.
    /// </summary>
    public ProgressionDto Progression { get; init; } = null!;

    /// <summary>
    /// What type of exercise is this variation?
    /// </summary>
    [Display(Name = "Section")]
    public Section Section { get; init; }

    /// <summary>
    /// What sports does performing this exercise benefit.
    /// </summary>
    [Display(Name = "Sports Focus", ShortName = "Sports")]
    public SportsFocus SportsFocus { get; init; }

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    /// <summary>
    /// Combination of this variations Strength, Stretch and Stability muscles worked.
    /// </summary>
    public MusculoskeletalSystem AllMuscles => Strengthens | Stretches | Stabilizes;

    public string? DefaultInstruction { get; init; }

    [UIHint(nameof(InstructionDto))]
    public virtual ICollection<InstructionDto> Instructions { get; init; } = [];

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is VariationDto other
        && other.Id == Id;

    public IOrderedEnumerable<InstructionDto> GetRootInstructions(UserNewsletterDto? user)
    {
        return Instructions
            // Only show the optional equipment groups that the user owns the equipment of.
            .Where(eg => user == null
                // Or the instruction doesn't have any equipment.
                || eg.Equipment == Equipment.None
                // Or the user owns the equipment of the root instruction.
                || (user.Equipment & eg.Equipment) != 0
                    // And the root instruction can be done on its own, or is an ordered difficulty.
                    // Or the user owns the equipment of the child instructions.
                    && (eg.Link != null || eg.Order != null || eg.GetChildInstructions(user).Any()))
            // Keep the order consistent across newsletters.
            .OrderByDescending(eg => eg.Children.Any() && !eg.Order.HasValue)
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
