﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises.
/// Variation-Exercise prerequisites?
/// </summary>
[Table("exercise_prerequisite")]
[DebuggerDisplay("{Exercise} needs {PrerequisiteExercise}")]
public class ExercisePrerequisite
{
    /// <summary>
    /// The Id of the postrequisite exercise.
    /// </summary>
    public virtual int ExerciseId { get; private init; }

    public virtual int PrerequisiteExerciseId { get; private init; }

    /// <summary>
    /// The postrequisite exercise.
    /// </summary>
    [JsonIgnore, InverseProperty(nameof(Entities.Exercise.Exercise.Prerequisites))]
    public virtual Exercise Exercise { get; private init; } = null!;

    [InverseProperty(nameof(Entities.Exercise.Exercise.Postrequisites))]
    public virtual Exercise PrerequisiteExercise { get; private init; } = null!;

    /// <summary>
    /// The progression level of the prerequisite the user needs to be at to unlock the postrequisite.
    /// </summary>
    [DefaultValue(50)]
    [Required, Range(UserConsts.UserProgressionMin, UserConsts.UserProgressionMax)]
    public int Proficiency { get; private init; }
}
