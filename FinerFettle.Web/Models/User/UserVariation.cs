using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// User's intensity stats.
    /// </summary>
    [Table("user_variation"), Comment("User's intensity stats")]
    public class UserVariation
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VariationId { get; set; }

        [Required]
        public Exercise.Variation Variation { get; set; } = null!;

        [Required]
        public User User { get; set; } = null!;

        public DateOnly LastSeen { get; set; }
    }
}
