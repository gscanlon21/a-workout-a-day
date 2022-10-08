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

        public string? DisabledReason { get; set; } = null;

        /// <summary>
        /// The progression level needed to attain proficiency in the exercise
        /// </summary>
        [Range(5, 95)]
        public int Proficiency { get; set; }

        /// <summary>
        /// Primary muscles (usually strengthening) worked by the exercise
        /// </summary>
        [Required]
        public MuscleGroups PrimaryMuscles { get; set; }

        /// <summary>
        /// Secondary (usually stabilizing) muscles worked by the exercise
        /// </summary>
        [Required]
        public MuscleGroups SecondaryMuscles { get; set; }

        [NotMapped]
        public MuscleGroups AllMuscles => PrimaryMuscles | SecondaryMuscles;

        [InverseProperty(nameof(ExercisePrerequisite.Exercise))]
        public virtual ICollection<ExercisePrerequisite> Prerequisites { get; set; } = default!;
        [InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
        public virtual ICollection<ExercisePrerequisite> Exercises { get; set; } = default!;

        [InverseProperty(nameof(Variation.Exercise))]
        public virtual ICollection<Variation> Variations { get; set; } = default!;

        [InverseProperty(nameof(User.ExerciseUserProgression.Exercise))]
        public virtual ICollection<ExerciseUserProgression> UserProgressions { get; set; } = null!;
    }
}
