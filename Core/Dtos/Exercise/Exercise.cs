using Core.Dtos.User;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;


namespace Core.Dtos.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
public interface IExercise
{
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; }

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required]
    public int Proficiency { get; init; }

    /// <summary>
    /// Similar groups of exercises.
    /// </summary>
    [Required]
    public ExerciseGroup Groups { get; init; }

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; init; }

    public string? DisabledReason { get; init; }

    //[JsonIgnore, InverseProperty(nameof(ExercisePrerequisite.Exercise))]
    public ICollection<IExercisePrerequisite> Prerequisites { get; init; }

    //[JsonIgnore, InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
    public ICollection<IExercisePrerequisite> PrerequisiteExercises { get; init; }

    //[JsonIgnore, InverseProperty(nameof(ExerciseVariation.Exercise))]
    public ICollection<IExerciseVariation> ExerciseVariations { get; init; }

    //[JsonIgnore, InverseProperty(nameof(UserExercise.Exercise))]
    public ICollection<IUserExercise> UserExercises { get; init; }
}
