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
        public int Id { get; init; }

        [Required]
        public DateOnly Date { get; init; }

        [Required]
        public ExerciseRotaion ExerciseRotation { get; init; } = null!;

        public User.User? User { get; set; }
    }
}
