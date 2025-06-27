using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Equipment;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

// TODO: Implement IValidatableObject and setup model validation instead of using the /exercises/check route
/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("variation")]
[DebuggerDisplay("{Name,nq}")]
public class Variation
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; private init; } = null!;

    /// <summary>
    /// The filename.ext of the static content image
    /// </summary>
    [Required]
    public string StaticImage { get; private init; } = null!;

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
    public MuscleMovement MuscleMovement { get; private init; }

    /// <summary>
    /// What functional movement patterns does this variation work?
    /// </summary>
    [Required]
    public MovementPattern MovementPattern { get; private init; }

    /// <summary>
    /// Primary muscles strengthened by the exercise.
    /// </summary>
    [Required]
    public MusculoskeletalSystem Strengthens { get; private init; }

    /// <summary>
    /// Primary muscles stretched by the exercise.
    /// </summary>
    [Required]
    public MusculoskeletalSystem Stretches { get; private init; }

    /// <summary>
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    [Required]
    public MusculoskeletalSystem Stabilizes { get; private init; }

    /// <summary>
    /// What is this variation focusing on?
    /// </summary>
    [Required]
    [Display(Name = "Exercise Focus", ShortName = "Focus")]
    public ExerciseFocus ExerciseFocus { get; private init; }

    public virtual int ExerciseId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Exercise.Variations))]
    public virtual Exercise Exercise { get; private init; } = null!;

    /// <summary>
    /// The progression range required to view the exercise variation.
    /// </summary>
    [Required]
    public Progression Progression { get; private init; } = null!;

    /// <summary>
    /// What type of exercise is this variation?
    /// </summary>
    [Required]
    [Display(Name = "Section")]
    public Section Section { get; private init; }

    /// <summary>
    /// What sports does performing this exercise benefit.
    /// </summary>
    [Required]
    [Display(Name = "Sports Focus", ShortName = "Sports")]
    public SportsFocus SportsFocus { get; private init; }

    public string? DisabledReason { get; private init; } = null;

    /// <summary>
    /// Notes about the variation (externally shown)
    /// </summary>
    public string? Notes { get; private init; } = null;

    /// <summary>
    /// Combination of this variations Strength, Stretch and Stability muscles worked.
    /// </summary>
    [NotMapped]
    public MusculoskeletalSystem AllMuscles => Strengthens | Stretches | Stabilizes;

    public string? DefaultInstruction { get; private init; }

    // Cannot have an InverseProperty because we have two navigation properties to Instruction
    [UIHint(nameof(Instruction))] //[JsonIgnore, InverseProperty(nameof(Instruction.Variation))]
    public virtual ICollection<Instruction> Instructions { get; private init; } = [];

    [JsonIgnore, InverseProperty(nameof(UserVariation.Variation))]
    public virtual ICollection<UserVariation> UserVariations { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserWorkoutVariation.Variation))]
    public virtual ICollection<UserWorkoutVariation> UserWorkoutVariations { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is Variation other
        && other.Id == Id;

    public Proficiency? GetProficiency(Section section, Intensity intensity)
    {
        int GetEnduranceSets(int sets)
        {
            return ExerciseFocus.HasFlag(ExerciseFocus.Endurance) ? sets + 1 : sets;
        }

        return section switch
        {
            Section.None => null,
            Section.CooldownStretching => new Proficiency(30, 60, null, null),
            Section.CooldownRelaxation => new Proficiency(300, 300, null, null),

            Section.WarmupPotentiation => new Proficiency(30, 60, null, null),
            Section.WarmupRaise => new Proficiency(60, 300, null, null),
            Section.WarmupActivation
            or Section.WarmupMobilization
            or Section.WarmupActivationMobilization => PauseReps switch
            {
                true => new Proficiency(3, 3, 10, 10),
                false => new Proficiency(null, null, 10, 10),
                null => new Proficiency(30, 30, null, null),
            },

            Section.PrehabStretching => new Proficiency(30, 60, null, null),
            Section.PrehabStrengthening
            or Section.PrehabStabilization => PauseReps switch
            {
                true => new Proficiency(3, 3, 10, 10) { Sets = 3 },
                false => new Proficiency(null, null, 10, 10) { Sets = 3 },
                null => new Proficiency(30, 30, null, null) { Sets = 3 },
            },

            Section.RehabVelocity => new Proficiency(30, 60, null, null),
            Section.RehabStrengthening => PauseReps switch
            {
                true => new Proficiency(3, 3, 10, 10) { Sets = 3 },
                false => new Proficiency(null, null, 10, 10) { Sets = 3 },
                null => new Proficiency(30, 30, null, null) { Sets = 3 },
            },
            Section.RehabMechanics => PauseReps switch
            {
                true => new Proficiency(3, 3, 10, 10),
                false => new Proficiency(null, null, 10, 10),
                null => new Proficiency(30, 30, null, null),
            },

            _ => intensity switch
            {
                Intensity.Endurance => PauseReps switch
                {
                    true => new Proficiency(1, 1, 15, 20) { Sets = GetEnduranceSets(1) },
                    false => new Proficiency(null, null, 15, 20) { Sets = GetEnduranceSets(1) },
                    null => new Proficiency(12, 24, null, null) { Sets = 5 },
                },
                Intensity.Light => PauseReps switch
                {
                    true => new Proficiency(1, 1, 12, 15) { Sets = GetEnduranceSets(2) },
                    false => new Proficiency(null, null, 12, 15) { Sets = GetEnduranceSets(2) },
                    null => new Proficiency(15, 30, null, null) { Sets = 4 },
                },
                Intensity.Medium => PauseReps switch
                {
                    true => new Proficiency(1, 1, 8, 12) { Sets = GetEnduranceSets(3) },
                    false => new Proficiency(null, null, 8, 12) { Sets = GetEnduranceSets(3) },
                    null => new Proficiency(20, 40, null, null) { Sets = 3 },
                },
                Intensity.Heavy => PauseReps switch
                {
                    true => new Proficiency(1, 1, 6, 8) { Sets = GetEnduranceSets(4) },
                    false => new Proficiency(null, null, 6, 8) { Sets = GetEnduranceSets(4) },
                    null => new Proficiency(30, 60, null, null) { Sets = 2 },
                },
                _ => new Proficiency(0, 0, 0, 0)
            },
        };
    }
}

/// <summary>
/// The range of progressions an exercise is available for.
/// </summary>
[Owned]
public record Progression([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
{
    public int MinOrDefault => Min ?? 0;
    public int MaxOrDefault => Max ?? 100;
}
