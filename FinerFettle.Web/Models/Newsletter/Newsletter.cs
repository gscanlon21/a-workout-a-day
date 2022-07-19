using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Newsletter
{
    [Comment("A day's workout routine"), Table(nameof(Newsletter))]
    public class Newsletter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public User.User? User { get; set; }

        [Required]
        public ExerciseType ExerciseType { get; set; }

        public MuscleGroups? MuscleGroups { get; set; }
    }
}
