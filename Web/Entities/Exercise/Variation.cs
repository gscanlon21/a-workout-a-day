using Web.Entities.Equipment;
using Web.Entities.User;
using Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Web.Entities.Exercise;

/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("variation"), Comment("Variations of exercises")]
[DebuggerDisplay("{Name,nq}")]
public class Variation
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public string Name { get; private init; } = null!;

    [Required]
    public string ImageCode { get; private init; } = null!;

    [Required]
    public bool Unilateral { get; set; }

    /// <summary>
    /// Works against gravity. 
    /// 
    /// A pullup, a squat, a deadlift, a row....
    /// </summary>
    [Required]
    public bool AntiGravity { get; set; }

    [Required]
    public MuscleContractions MuscleContractions { get; private init; }

    [Required]
    public MuscleMovement MuscleMovement { get; private init; }

    [Required]
    public MovementPattern MovementPattern { get; private init; }

    /// <summary>
    /// Primary muscles (usually strengthening) worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StrengthMuscles { get; private init; }

    /// <summary>
    /// Primary muscles (usually strengthening) worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StretchMuscles { get; private init; }

    /// <summary>
    /// Secondary (usually stabilizing) muscles worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups StabilityMuscles { get; private init; }

    public string? DisabledReason { get; private init; } = null;
    
    /// <summary>
    /// Internal notes about the variation
    /// </summary>
    public string? Notes { get; private init; } = null;

    [NotMapped]
    public MuscleGroups AllMuscles => StrengthMuscles | StretchMuscles | StabilityMuscles;

    [InverseProperty(nameof(EquipmentGroup.Variation)), UIHint(nameof(EquipmentGroup))]
    public virtual ICollection<EquipmentGroup> EquipmentGroups { get; private init; } = new List<EquipmentGroup>();

    [InverseProperty(nameof(UserVariation.Variation))]
    public virtual ICollection<UserVariation> UserVariations { get; private init; } = null!;

    [InverseProperty(nameof(Intensity.Variation))]
    public virtual List<Intensity> Intensities { get; private init; } = null!;

    [InverseProperty(nameof(ExerciseVariation.Variation))]
    public virtual ICollection<ExerciseVariation> ExerciseVariations { get; private init; } = null!;

    [InverseProperty(nameof(Newsletter.NewsletterVariation.Variation))]
    public virtual ICollection<Newsletter.NewsletterVariation> NewsletterVariations { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is Variation other
        && other.Id == Id;
}
