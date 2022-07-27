using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.User
{
    [Comment("User who signed up for the newsletter"), Table(nameof(User))]
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Range(0, 100)] 
        public int? Progression { get; set; } = 50; // FIXME: Magic int is magic. Really the middle progression level.

        [Required]
        public bool NeedsRest { get; set; }

        [Required]
        public bool OverMinimumAge { get; set; }

        [Required]
        public IList<EquipmentUser> EquipmentUsers { get; set; } = new List<EquipmentUser>();

        [Required]
        public RestDays RestDays { get; set; }

        // TODO? Many to many relationship with Exercise so user can filter certain exercises out
    }

    public class EquipmentUser
    {
        public int EquipmentId { get; set; }
        public int UserId { get; set; }

        public User User { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
    }
}
