using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.Exercise;

// TODO: Implement IValidateableObject and setup model validation instead of using the /exercises/check route
/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("variation")]
[DebuggerDisplay("{Name,nq}")]
public class VariationDto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups SecondaryMuscles { get; init; }

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
    public MuscleGroups AllMuscles => StrengthMuscles | StretchMuscles | SecondaryMuscles;

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

    public ProficiencyDto GetProficiency(Section section, Intensity intensity)
    {
        int GetEnduranceSets(int sets)
        {
            return ExerciseFocus.HasFlag(ExerciseFocus.Endurance) ? sets + 1 : sets;
        }

        return section switch
        {
            Section.CooldownStretching => new ProficiencyDto(30, 60, null, null),
            Section.Mindfulness => new ProficiencyDto(300, 300, null, null),

            Section.WarmupPotentiation => new ProficiencyDto(30, 60, null, null),
            Section.WarmupRaise => new ProficiencyDto(60, 300, null, null),
            Section.WarmupActivationMobilization => PauseReps switch
            {
                true => new ProficiencyDto(3, 3, 10, 10),
                false => new ProficiencyDto(null, null, 10, 10),
                null => new ProficiencyDto(30, 30, null, null),
            },

            Section.PrehabStretching => new ProficiencyDto(30, 60, null, null),
            Section.PrehabStrengthening => PauseReps switch
            {
                true => new ProficiencyDto(3, 3, 10, 10) { Sets = 3 },
                false => new ProficiencyDto(null, null, 10, 10) { Sets = 3 },
                null => new ProficiencyDto(30, 30, null, null) { Sets = 3 },
            },

            Section.RehabVelocity => new ProficiencyDto(30, 60, null, null),
            Section.RehabStrengthening => PauseReps switch
            {
                true => new ProficiencyDto(3, 3, 10, 10) { Sets = 3 },
                false => new ProficiencyDto(null, null, 10, 10) { Sets = 3 },
                null => new ProficiencyDto(30, 30, null, null) { Sets = 3 },
            },
            Section.RehabMechanics => PauseReps switch
            {
                true => new ProficiencyDto(3, 3, 10, 10),
                false => new ProficiencyDto(null, null, 10, 10),
                null => new ProficiencyDto(30, 30, null, null),
            },

            _ => intensity switch
            {
                Intensity.Endurance => PauseReps switch
                {
                    true => new ProficiencyDto(1, 1, 15, 20) { Sets = GetEnduranceSets(1) },
                    false => new ProficiencyDto(null, null, 15, 20) { Sets = GetEnduranceSets(1) },
                    null => new ProficiencyDto(12, 24, null, null) { Sets = 5 },
                },
                Intensity.Light => PauseReps switch
                {
                    true => new ProficiencyDto(1, 1, 12, 15) { Sets = GetEnduranceSets(2) },
                    false => new ProficiencyDto(null, null, 12, 15) { Sets = GetEnduranceSets(2) },
                    null => new ProficiencyDto(15, 30, null, null) { Sets = 4 },
                },
                Intensity.Medium => PauseReps switch
                {
                    true => new ProficiencyDto(1, 1, 8, 12) { Sets = GetEnduranceSets(3) },
                    false => new ProficiencyDto(null, null, 8, 12) { Sets = GetEnduranceSets(3) },
                    null => new ProficiencyDto(20, 40, null, null) { Sets = 3 },
                },
                Intensity.Heavy => PauseReps switch
                {
                    true => new ProficiencyDto(1, 1, 6, 8) { Sets = GetEnduranceSets(4) },
                    false => new ProficiencyDto(null, null, 6, 8) { Sets = GetEnduranceSets(4) },
                    null => new ProficiencyDto(30, 60, null, null) { Sets = 2 },
                },
                _ => new ProficiencyDto(0, 0, 0, 0)
            },
        };
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
