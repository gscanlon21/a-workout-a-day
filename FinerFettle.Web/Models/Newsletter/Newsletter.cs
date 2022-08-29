using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Newsletter
{
    /// <summary>
    /// A day's workout routine.
    /// </summary>
    [Comment("A day's workout routine"), Table(nameof(Newsletter))]
    public class Newsletter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public DateOnly Date { get; init; }

        [Required]
        public ExerciseRotaion ExerciseRotation { get; init; } = null!;

        public User.User? User { get; set; }
    }

    /// <summary>
    /// The detail shown in the newsletter.
    /// </summary>
    public enum Verbosity
    {
        Quiet = 1 << 0,
        Minimal = 1 << 1 | Quiet,
        Normal = 1 << 2 | Minimal,
        Detailed = 1 << 3 | Normal,
        Diagnostic = 1 << 4 | Detailed
    }
}
