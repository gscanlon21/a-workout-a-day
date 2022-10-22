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
    [Table("exercise_variation"), Comment("Variation progressions for an exercise track")]
    [DebuggerDisplay("{Exercise,nq}")]
    public class ExerciseVariation
    {
        [Required]
        public Progression Progression { get; set; } = null!;

        public virtual int ExerciseId { get; set; } = default!;

        [InverseProperty(nameof(Models.Exercise.Exercise.ExerciseVariations))]
        public virtual Exercise Exercise { get; set; } = null!;

        public virtual int VariationId { get; set; } = default!;

        [InverseProperty(nameof(Models.Exercise.Variation.ExerciseVariations))]
        public virtual Variation Variation { get; set; } = null!;
    }
}
