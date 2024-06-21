using Core.Consts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[Table("exercise_prerequisite")]
[DebuggerDisplay("{Exercise} needs {PrerequisiteExercise}")]
public class ExercisePrerequisiteDto
{
    public virtual int ExerciseId { get; init; }

    [JsonIgnore]
    public virtual ExerciseDto Exercise { get; init; } = null!;

    public virtual int PrerequisiteExerciseId { get; init; }

    public virtual ExerciseDto PrerequisiteExercise { get; init; } = null!;

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Proficiency { get; init; }

    /*
    public int Proficiency { get; set; } = exercisePrerequisite.Proficiency;
    public int Id { get; set; } = exercisePrerequisite.PrerequisiteExerciseId;
    public string Name { get; set; } = exercisePrerequisite.PrerequisiteExercise.Name;
    */
}
