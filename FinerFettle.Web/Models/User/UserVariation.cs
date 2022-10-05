using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// User's variation excluding.
    /// </summary>
    [Table(nameof(UserVariation)), Comment("User's variation excluding")]
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

        public int SeenCount { get; set; }
    }
}
