using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.Exercise;

/// <summary>
/// Pre-requisite exercises for other exercises
/// </summary>
[Table("exercise_prerequisite"), Comment("Pre-requisite exercises for other exercises")]
[DebuggerDisplay("Name = {Name}")]
public class ExercisePrerequisite
{
    public virtual int ExerciseId { get; private init; }

    [InverseProperty(nameof(Entities.Exercise.Exercise.Prerequisites))]
    public virtual Exercise Exercise { get; private init; } = null!;

    public virtual int PrerequisiteExerciseId { get; private init; }

    [InverseProperty(nameof(Entities.Exercise.Exercise.PrerequisiteExercises))]
    public virtual Exercise PrerequisiteExercise { get; private init; } = null!;
}
