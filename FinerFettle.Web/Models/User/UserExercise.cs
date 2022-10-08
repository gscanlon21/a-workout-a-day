using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// User's progression level of an exercise.
    /// </summary>
    [Table("user_exercise"), Comment("User's progression level of an exercise")]
    public class UserExercise
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
        public int Progression { get; set; }

        /// <summary>
        /// Don't show this exercise or any of it's variations to the user
        /// </summary>
        public bool Ignore { get; set; } = false;

        public DateOnly LastSeen { get; set; }
    }
}
