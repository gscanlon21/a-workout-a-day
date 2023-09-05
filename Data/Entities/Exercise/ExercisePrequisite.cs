using Core.Consts;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[Table("exercise_prerequisite"), Comment("Pre-requisite exercises for other exercises")]
[DebuggerDisplay("{Exercise} needs {PrerequisiteExercise}")]
public class ExercisePrerequisite
{
    public virtual int ExerciseId { get; private init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Exercise.Prerequisites))]
    public virtual Exercise Exercise { get; private init; } = null!;

    public virtual int PrerequisiteExerciseId { get; private init; }

    [InverseProperty(nameof(Entities.Exercise.Exercise.Postrequisites))]
    public virtual Exercise PrerequisiteExercise { get; private init; } = null!;

    /// <summary>
    /// The progression level needed to attain proficiency in the exercise
    /// </summary>
    [Required, Range(UserConsts.MinUserProgression, UserConsts.MaxUserProgression)]
    public int Proficiency { get; private init; }
}
