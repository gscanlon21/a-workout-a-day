using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Models.Exercise;
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

        [Required]
        public Equipment Equipment { get; set; }

        [Required]
        public RestDays RestDays { get; set; }

        [NotMapped]
        public Equipment[]? EquipmentBinder { 
            get => Enum.GetValues<Equipment>().Cast<Equipment>().Where(e => Equipment.HasFlag(e)).ToArray();
            set => Equipment = value?.Aggregate(Equipment.None, (a, e) => a | e) ?? Equipment.None; 
        }

        [NotMapped]
        public RestDays[]? RestDaysBinder {
            get => Enum.GetValues<RestDays>().Cast<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
            set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
        }
    }
}
