using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Exercises listed on the website
    /// </summary>
    [Table(nameof(Exercise)), Comment("Exercises listed on the website")]
    [DebuggerDisplay("Name = {Name}")]
    public class Exercise
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Primary muscles worked by the exercise
        /// </summary>
        [Required]
        public MuscleGroups Muscles { get; set; }

        ///// <summary>
        ///// Secondary muscles worked by the exercise. Stabilizing muscles? What are these for stability exercises?
        ///// </summary>
        //[Required]
        //public MuscleGroups SecondaryMuscles { get; set; }

        [InverseProperty(nameof(Variation.Exercise))]
        public virtual ICollection<Variation> Variations { get; set; } = default!;

        [InverseProperty(nameof(User.ExerciseUserProgression.Exercise))]
        public virtual ICollection<ExerciseUserProgression> UserProgressions { get; set; } = null!;
    }
}
