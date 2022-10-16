using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Newsletter
{
    /// <summary>
    /// A day's workout routine.
    /// </summary>
    [Table("newsletter"), Comment("A day's workout routine")]
    public class Newsletter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        /// <summary>
        /// The date the newsletter was sent out on
        /// </summary>
        [Required]
        public DateOnly Date { get; init; }

        [Required]
        public ExerciseRotation ExerciseRotation { get; init; } = null!;

        [InverseProperty(nameof(Models.User.User.Newsletters))]
        public User.User? User { get; set; }

        /// <summary>
        /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
        /// </summary>
        [Required]
        public bool IsDeloadWeek { get; init; }
    }
}
