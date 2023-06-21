using Lib.Dtos.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Lib.Dtos.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
[Table("exercise")]
[DebuggerDisplay("{Name,nq}")]
public class Exercise
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required, Range(UserExercise.MinUserProgression, UserExercise.MaxUserProgression)]
    public int Proficiency { get; init; }

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    [Required]
    public ExerciseGroup Groups { get; init; }

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; } = null;

    public string? DisabledReason { get; init; } = null;

    //[JsonIgnore, InverseProperty(nameof(ExercisePrerequisite.Exercise))]
    public virtual ICollection<ExercisePrerequisite> Prerequisites { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
    public virtual ICollection<ExercisePrerequisite> PrerequisiteExercises { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(ExerciseVariation.Exercise))]
    public virtual ICollection<ExerciseVariation> ExerciseVariations { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(UserExercise.Exercise))]
    public virtual ICollection<UserExercise> UserExercises { get; init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is Exercise other
        && other.Id == Id;
}
