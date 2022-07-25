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

        [MustBeTrue]
        public bool OverMinimumAge { get; set; }

        //[Required] FIXME: ModelState validation for this prop since it doesn't get assigned to until controller action
        [NotMapped]
        public IList<Equipment> Equipment { get; set; }

        public IList<EquipmentUser> EquipmentUsers { get; set; }

        [Required]
        public RestDays RestDays { get; set; }

        // TODO? Many to many relationship with Exercise so user can filter certain exercises out

        [NotMapped]
        public int[]? EquipmentBinder { get; set; }

        [NotMapped]
        public RestDays[]? RestDaysBinder {
            get => Enum.GetValues<RestDays>().Cast<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
            set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
        }
    }

    public class EquipmentUser
    {
        public int EquipmentId { get; set; }
        public int UserId { get; set; }

        public User User { get; set; }
        public Equipment Equipment { get; set; }
    }
}
