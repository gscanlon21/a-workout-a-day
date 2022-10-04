using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// User's intensity stats.
    /// </summary>
    [Table(nameof(UserIntensity)), Comment("User's intensity stats")]
    public class UserIntensity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int IntensityId { get; set; }

        [Required]
        public Exercise.Intensity Intensity { get; set; } = null!;

        [Required]
        public User User { get; set; } = null!;

        public int SeenCount { get; set; }
    }
}
