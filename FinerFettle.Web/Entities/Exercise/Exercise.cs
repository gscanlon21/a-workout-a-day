using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
[Table("exercise"), Comment("Exercises listed on the website")]
[DebuggerDisplay("Name = {Name}")]
public class Exercise
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public string Name { get; private init; } = null!;

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required, Range(UserExercise.MinUserProgression, UserExercise.MaxUserProgression)]
    public int Proficiency { get; private init; }

    /// <summary>
    /// Primary muscles (usually strengthening) worked by the exercise
    /// </summary>
    [Required]
    public MuscleGroups RecoveryMuscle { get; private init; }

    [Required]
    public SportsFocus SportsFocus { get; private init; }

    public string? DisabledReason { get; private init; } = null;

    [InverseProperty(nameof(ExercisePrerequisite.Exercise))]
    public virtual ICollection<ExercisePrerequisite> Prerequisites { get; private init; } = null!;

    [InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
    public virtual ICollection<ExercisePrerequisite> PrerequisiteExercises { get; private init; } = null!;

    [InverseProperty(nameof(ExerciseVariation.Exercise))]
    public virtual ICollection<ExerciseVariation> ExerciseVariations { get; private init; } = null!;

    [InverseProperty(nameof(UserExercise.Exercise))]
    public virtual ICollection<UserExercise> UserExercises { get; private init; } = null!;
}

public class ExerciseComparer : IEqualityComparer<Exercise>
{
    public bool Equals(Exercise? a, Exercise? b)
    {
        return a?.Id == b?.Id;
    }

    public int GetHashCode(Exercise a)
    {
        int hash = 17;
        hash = hash * 23 + a.Id.GetHashCode();
        return hash;
    }
}
