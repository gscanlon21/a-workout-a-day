using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Core.Models.User;
using Lib.Pages.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.Pages.Shared.Exercise;


[DebuggerDisplay("{Exercise,nq}: {Variation,nq}")]
public class ExerciseVariationViewModel
{
    public Section Section { get; init; }

    public ExerciseViewModel Exercise { get; init; } = null!;

    public VariationViewModel Variation { get; init; } = null!;

    [JsonInclude]
    public UserExerciseViewModel? UserExercise { get; set; }

    [JsonInclude]
    public UserVariationViewModel? UserVariation { get; set; }

    public bool UserFirstTimeViewing { get; init; } = false;

    public string? EasierVariation { get; init; }
    public string? HarderVariation { get; init; }

    public string? EasierReason { get; init; }
    public string? HarderReason { get; init; }

    public ProficiencyViewModel? Proficiency { get; init; }

    [JsonInclude]
    public ICollection<ExercisePrerequisiteViewModel> ExercisePrerequisites { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Exercise, Variation);

    public override bool Equals(object? obj) => obj is ExerciseVariationViewModel other
        && other.Exercise == Exercise && other.Variation == Variation;
}


/// <summary>
/// User's progression level of an exercise.
/// </summary>
[DebuggerDisplay("User: {UserId}, Exercise: {ExerciseId}")]
public class UserExerciseViewModel
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int ExerciseId { get; init; }

    /// <summary>
    /// How far the user has progressed for this exercise.
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Progression { get; set; } = UserConsts.MinUserProgression;

    /// <summary>
    /// Don't show this exercise or any of it's variations to the user
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    /// <summary>
    /// When was this exercise last seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastSeen { get; set; }

    /// <summary>
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    [JsonInclude]
    public ExerciseViewModel Exercise { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(UserId, ExerciseId);

    public override bool Equals(object? obj) => obj is UserExerciseViewModel other
        && other.ExerciseId == ExerciseId
        && other.UserId == UserId;
}


/// <summary>
/// User's intensity stats.
/// </summary>
[DebuggerDisplay("User: {UserId}, Variation: {VariationId}")]
public class UserVariationViewModel
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int VariationId { get; init; }

    /// <summary>
    /// Don't show this variation to the user.
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    public string? Notes { get; init; }

    /// <summary>
    /// When was this exercise last seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastSeen { get; set; }

    /// <summary>
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    public int Weight { get; set; }

    public int Sets { get; set; }

    public int Reps { get; set; }

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId);

    public override bool Equals(object? obj) => obj is UserVariationViewModel other
        && other.VariationId == VariationId
        && other.UserId == UserId;
}



/// <summary>
/// Exercises listed on the website
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class ExerciseViewModel
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// The type of skills.
    /// </summary>
    public SkillTypes SkillType { get; init; }

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    public int Skills { get; init; }

    public Enum UnusedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => VisualSkills.All & ~(VisualSkills)Skills,
        _ => WorkoutSkills.All & ~(WorkoutSkills)Skills,
    };

    public Enum TypedSkills => SkillType switch
    {
        SkillTypes.VisualSkills => (VisualSkills)Skills,
        _ => (WorkoutSkills)Skills,
    };

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public string? DisabledReason { get; init; } = null;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is ExerciseViewModel other
        && other.Id == Id;
}

// TODO: Implement IValidateableObject and setup model validation instead of using the /exercises/check route
/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class VariationViewModel
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
    /// What sports does performing this exercise benefit.
    /// </summary>
    [Required]
    public SportsFocus SportsFocus { get; init; }

    /// <summary>
    /// Where in the newsletter should this exercise be shown.
    /// </summary>
    [Required]
    public ExerciseFocus ExerciseFocus { get; init; }

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
    /// Primary joints strengthened by the exercise
    /// </summary>
    [Required]
    public Joints MobilityJoints { get; init; }

    /// <summary>
    /// Primary muscles strengthened by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StrengthMuscles { get; init; }

    /// <summary>
    /// Primary muscles stretched by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StretchMuscles { get; init; }

    /// <summary>
    /// The progression range required to view the exercise variation
    /// </summary>
    [Required]
    public Progression Progression { get; init; } = null!;

    public int ExerciseId { get; init; }

    [JsonInclude]
    public ExerciseViewModel Exercise { get; init; } = null!;

    /// <summary>
    /// Where in the newsletter should this exercise be shown.
    /// </summary>
    [Required]
    public Section Section { get; init; }

    /// <summary>
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups SecondaryMuscles { get; init; }

    public string? DisabledReason { get; init; } = null;

    /// <summary>
    /// Notes about the variation (externally shown)
    /// </summary>
    public string? Notes { get; init; } = null;

    /// <summary>
    /// Combination of this variations Strength, Stretch and Stability muscles worked.
    /// </summary>
    public MuscleGroups AllMuscles => StrengthMuscles | StretchMuscles | SecondaryMuscles;

    public string? DefaultInstruction { get; init; }

    [JsonInclude]
    public ICollection<InstructionViewModel2> Instructions { private get; init; } = [];

    public bool HasRootInstructions => Instructions.Any();

    public IOrderedEnumerable<InstructionViewModel2> GetRootInstructions(UserNewsletterViewModel? user)
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

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is VariationViewModel other
        && other.Id == Id;
}

/// <summary>
/// The range of progressions an exercise is available for.
/// </summary>
public record Progression([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
{
    public int MinOrDefault => Min ?? 0;
    public int MaxOrDefault => Max ?? 100;
}
