using App.Dtos.Equipment;
using App.Dtos.User;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace App.Dtos.Exercise;

// TODO: Implement IValidateableObject and setup model validation instead of using the /exercises/check route
/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("variation")]
[DebuggerDisplay("{Name,nq}")]
public class Variation
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
    /// Works against gravity. 
    /// 
    /// A pullup, a squat, a deadlift, a row....
    /// </summary>
    [Required]
    public bool AntiGravity { get; set; }

    /// <summary>
    /// Can the variation be performed with weights?
    /// 
    /// This controls whether the Pounds selector shows to the user.
    /// </summary>
    [Required]
    public bool IsWeighted { get; set; }

    /// <summary>
    /// Does this variation work muscles by moving weights or holding them in place?
    /// </summary>
    [Required]
    public MuscleContractions MuscleContractions { get; init; }

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
    /// Where in the newsletter should this exercise be shown.
    /// </summary>
    [Required]
    public ExerciseFocus ExerciseFocus { get; init; }

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

    public int? DefaultInstructionId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Instruction.Variation))]
    public virtual Instruction? DefaultInstruction { get; init; }

    [UIHint(nameof(Instruction))] //[JsonIgnore, InverseProperty(nameof(Instruction.Variation))]
    public virtual ICollection<Instruction> Instructions { get; init; } = new List<Instruction>();

    [JsonIgnore, InverseProperty(nameof(UserVariation.Variation))]
    public virtual ICollection<UserVariation> UserVariations { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Intensity.Variation))]
    public virtual List<Intensity> Intensities { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(ExerciseVariation.Variation))]
    public virtual ICollection<ExerciseVariation> ExerciseVariations { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is Variation other
        && other.Id == Id;
}
