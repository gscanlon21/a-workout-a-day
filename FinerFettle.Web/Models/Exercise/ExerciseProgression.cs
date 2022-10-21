using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Intensity level of an exercise variation
    /// </summary>
    [Table("exercise_progression"), Comment("Variation progressions for an exercise track")]
    [DebuggerDisplay("{Exercise,nq}")]
    public class ExerciseProgression
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public Progression Progression { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Exercise.ExerciseProgressions))]
        public virtual Exercise Exercise { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Variation.ExerciseProgressions))]
        public virtual Variation Variation { get; set; } = null!;
    }
}
