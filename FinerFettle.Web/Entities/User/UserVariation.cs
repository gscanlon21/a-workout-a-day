using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Entities.User
{
    /// <summary>
    /// User's intensity stats.
    /// </summary>
    [Table("user_variation"), Comment("User's intensity stats")]
    public class UserVariation
    {
        [Required]
        public int UserId { get; init; }

        [Required]
        public int VariationId { get; init; }

        [Required]
        public DateOnly LastSeen { get; set; }

        [InverseProperty(nameof(Entities.User.User.UserVariations))]
        public virtual User User { get; private init; } = null!;

        [InverseProperty(nameof(Exercise.Variation.UserVariations))]
        public virtual Exercise.Variation Variation { get; private init; } = null!;
    }
}
