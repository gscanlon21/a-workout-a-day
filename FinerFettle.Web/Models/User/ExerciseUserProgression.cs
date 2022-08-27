using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.User
{
    [Table(nameof(ExerciseUserProgression)), Comment("User's progression level of an exercise")]
    public class ExerciseUserProgression
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        [Required]
        public Exercise.Exercise Exercise { get; set; } = null!;

        [Required]
        public User User { get; set; } = null!;

        [Range(5, 95)]
        public int Progression { get; set; } = 50; // FIXME: Magic int is magic. Really the middle progression level.
    }
}
