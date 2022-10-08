using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Pre-requisite exercises for other exercises
    /// </summary>
    [Table("exercise_prerequisite"), Comment("Pre-requisite exercises for other exercises")]
    [DebuggerDisplay("Name = {Name}")]
    public class ExercisePrerequisite
    {
        public virtual int ExerciseId { get; set; } = default!;
        [InverseProperty(nameof(Models.Exercise.Exercise.Prerequisites))]
        public virtual Exercise Exercise { get; set; } = default!;

        public virtual int PrerequisiteExerciseId { get; set; } = default!;
        [InverseProperty(nameof(Models.Exercise.Exercise.Exercises))]
        public virtual Exercise PrerequisiteExercise { get; set; } = default!;
    }
}
