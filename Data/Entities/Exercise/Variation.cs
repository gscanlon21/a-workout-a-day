using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Equipment;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

// TODO: Implement IValidateableObject and setup model validation instead of using the /exercises/check route
/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("variation"), Comment("Variations of exercises")]
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
    /// Primary joints strengthened by the exercise
    /// </summary>
    [Required]
    public Joints MobilityJoints { get; private init; }

    /// <summary>
    /// Primary muscles strengthened by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StrengthMuscles { get; private init; }

    /// <summary>
    /// Primary muscles stretched by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StretchMuscles { get; private init; }

    /// <summary>
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups SecondaryMuscles { get; private init; }

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
    public MuscleGroups AllMuscles => StrengthMuscles | StretchMuscles | SecondaryMuscles;

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

    public Proficiency GetProficiency(Section section, Intensity intensity, ExerciseFocus focus, bool needsDeload)
    {
        intensity = (needsDeload, section, intensity) switch
        {
            // Section-specific first
            (_, Section.Accessory, Intensity.Light) => Intensity.Endurance,
            (_, Section.Accessory, Intensity.Medium) => Intensity.Light,
            (_, Section.Accessory, Intensity.Heavy) => Intensity.Medium,

            (true, _, Intensity.Light) => Intensity.Endurance,
            (true, _, Intensity.Medium) => Intensity.Light,
            (true, _, Intensity.Heavy) => Intensity.Medium,

            _ => intensity,
        };

        return (PauseReps, section, intensity, focus.HasFlag(ExerciseFocus.Endurance)) switch
        {
            // Section-specific first
            (_, Section.CooldownStretching, _, _) => new Proficiency(30, 60, null, null),
            (_, Section.Mindfulness, _, _) => new Proficiency(300, 300, null, null),
            (_, Section.WarmupRaise, _, _) => new Proficiency(60, 300, null, null),
            (_, Section.WarmupPotentiation, _, _) => new Proficiency(30, 60, null, null),
            (_, Section.RehabVelocity, _, _) => new Proficiency(30, 60, null, null),
            (_, Section.PrehabStretching, _, _) => new Proficiency(30, 60, null, null),

            (true, Section.WarmupActivationMobilization, _, _) => new Proficiency(3, 3, 10, 10),
            (false, Section.WarmupActivationMobilization, _, _) => new Proficiency(null, null, 10, 10),
            (null, Section.WarmupActivationMobilization, _, _) => new Proficiency(30, 30, null, null),

            (true, Section.RehabMechanics, _, _) => new Proficiency(3, 3, 10, 10),
            (false, Section.RehabMechanics, _, _) => new Proficiency(null, null, 10, 10),
            (null, Section.RehabMechanics, _, _) => new Proficiency(30, 30, null, null),

            (true, Section.PrehabStrengthening, _, _) => new Proficiency(3, 3, 10, 10) { Sets = 3 },
            (false, Section.PrehabStrengthening, _, _) => new Proficiency(null, null, 10, 10) { Sets = 3 },
            (null, Section.PrehabStrengthening, _, _) => new Proficiency(30, 30, null, null) { Sets = 3 },

            (true, Section.RehabStrengthening, _, _) => new Proficiency(3, 3, 10, 10) { Sets = 3 },
            (false, Section.RehabStrengthening, _, _) => new Proficiency(null, null, 10, 10) { Sets = 3 },
            (null, Section.RehabStrengthening, _, _) => new Proficiency(30, 30, null, null) { Sets = 3 },

            (true, _, Intensity.Heavy, true) => new Proficiency(1, 1, 8, 12) { Sets = 5 },
            (true, _, Intensity.Medium, true) => new Proficiency(1, 1, 12, 15) { Sets = 4 },
            (true, _, Intensity.Light, true) => new Proficiency(1, 1, 15, 20) { Sets = 3 },
            (true, _, Intensity.Endurance, true) => new Proficiency(1, 1, 20, 30) { Sets = 2 },
            (false, _, Intensity.Heavy, true) => new Proficiency(null, null, 8, 12) { Sets = 5 },
            (false, _, Intensity.Medium, true) => new Proficiency(null, null, 12, 15) { Sets = 4 },
            (false, _, Intensity.Light, true) => new Proficiency(null, null, 15, 20) { Sets = 3 },
            (false, _, Intensity.Endurance, true) => new Proficiency(null, null, 20, 30) { Sets = 2 },

            (true, _, Intensity.Heavy, false) => new Proficiency(1, 1, 6, 8) { Sets = 4 },
            (true, _, Intensity.Medium, false) => new Proficiency(1, 1, 8, 12) { Sets = 3 },
            (true, _, Intensity.Light, false) => new Proficiency(1, 1, 12, 15) { Sets = 2 },
            (true, _, Intensity.Endurance, false) => new Proficiency(1, 1, 15, 20) { Sets = 1 },
            (false, _, Intensity.Heavy, false) => new Proficiency(null, null, 6, 8) { Sets = 4 },
            (false, _, Intensity.Medium, false) => new Proficiency(null, null, 8, 12) { Sets = 3 },
            (false, _, Intensity.Light, false) => new Proficiency(null, null, 12, 15) { Sets = 2 },
            (false, _, Intensity.Endurance, false) => new Proficiency(null, null, 15, 20) { Sets = 1 },

            (null, _, Intensity.Heavy, _) => new Proficiency(30, 60, null, null) { Sets = 2 },
            (null, _, Intensity.Medium, _) => new Proficiency(20, 40, null, null) { Sets = 3 },
            (null, _, Intensity.Light, _) => new Proficiency(15, 30, null, null) { Sets = 4 },
            (null, _, Intensity.Endurance, _) => new Proficiency(12, 24, null, null) { Sets = 5 },

            _ => new Proficiency(0, 0, 0, 0)
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
