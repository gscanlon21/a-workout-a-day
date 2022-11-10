using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.Entities.Exercise;

/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("exercise_variation"), Comment("Variation progressions for an exercise track")]
[Index(nameof(ExerciseId), nameof(VariationId), IsUnique = true)]
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public class ExerciseVariation
{
    private string GetDebuggerDisplay()
    {
        if (Variation != null && Exercise != null)
        {
            return $"{Exercise.Name}: {Variation.Name}";
        }

        return $"{ExerciseId}: {VariationId}";
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public Progression Progression { get; private init; } = null!;

    [Required]
    public bool IsBonus { get; private init; }

    [Required]
    public ExerciseType ExerciseType { get; private init; }

    public virtual int ExerciseId { get; private init; }

    [InverseProperty(nameof(Entities.Exercise.Exercise.ExerciseVariations))]
    public virtual Exercise Exercise { get; private init; } = null!;

    public virtual int VariationId { get; private init; }

    [InverseProperty(nameof(Entities.Exercise.Variation.ExerciseVariations))]
    public virtual Variation Variation { get; private init; } = null!;

    [InverseProperty(nameof(UserExerciseVariation.ExerciseVariation))]
    public virtual ICollection<UserExerciseVariation> UserExerciseVariations { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is ExerciseVariation other
        && other.Id == Id;
}

/// <summary>
/// The range of progressions an exercise is available for.
/// </summary>
[Owned]
public record Progression([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
{
    public int GetMinOrDefault => Min ?? 0;
    public int GetMaxOrDefault => Max ?? 100;
}
