using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Comment("Exercises listed on the website"), Table(nameof(Exercise))]
    public class Exercise
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public MuscleGroups Muscles { get; set; }

        [Required]
        public ExerciseType ExerciseType { get; set; }

        public IList<Variation> Variations { get; set; } = default!;
    }
}
