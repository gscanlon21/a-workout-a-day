using Core.Models.Exercise;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("exercise_variation"), Comment("Variation progressions for an exercise track")]
[Index(nameof(VariationId), nameof(ExerciseId), nameof(ExerciseType), IsUnique = true)]
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

    /// <summary>
    /// The progression range required to view the exercise variation.
    /// </summary>
    [Required]
    public Progression Progression { get; private init; } = null!;

    /// <summary>
    /// What type of exercise is this variation?
    /// </summary>
    [Required]
    [Display(Name = "Exercise Type", ShortName = "Type")]
    public ExerciseType ExerciseType { get; private init; }

    public string? DisabledReason { get; private init; } = null;

    /// <summary>
    /// Notes about the variation (externally shown)
    /// </summary>
    public string? Notes { get; private init; } = null;

    public virtual int ExerciseId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Exercise.ExerciseVariations))]
    public virtual Exercise Exercise { get; private init; } = null!;

    public virtual int VariationId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Variation.ExerciseVariations))]
    public virtual Variation Variation { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserExerciseVariation.ExerciseVariation))]
    public virtual ICollection<UserExerciseVariation> UserExerciseVariations { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Newsletter.UserWorkoutExerciseVariation.ExerciseVariation))]
    public virtual ICollection<Newsletter.UserWorkoutExerciseVariation> UserWorkoutExerciseVariations { get; private init; } = null!;

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
    public int MinOrDefault => Min ?? 0;
    public int MaxOrDefault => Max ?? 100;
}
