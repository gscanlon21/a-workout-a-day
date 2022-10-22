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
    [Table("exercise"), Comment("Exercises listed on the website")]
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
        [Range(UserExercise.MinUserProgression, UserExercise.MaxUserProgression)]
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

        [Required]
        public bool IsRecovery { get; set; } = false;

        [NotMapped]
        public MuscleGroups AllMuscles => PrimaryMuscles | SecondaryMuscles;

        [InverseProperty(nameof(ExercisePrerequisite.Exercise))]
        public virtual ICollection<ExercisePrerequisite> Prerequisites { get; set; } = default!;

        [InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
        public virtual ICollection<ExercisePrerequisite> PrerequisiteExercises { get; set; } = default!;

        [InverseProperty(nameof(ExerciseVariation.Exercise))]
        public virtual ICollection<ExerciseVariation> ExerciseVariations { get; set; } = default!;

        [InverseProperty(nameof(UserExercise.Exercise))]
        public virtual ICollection<UserExercise> UserExercises { get; set; } = null!;
    }
}
