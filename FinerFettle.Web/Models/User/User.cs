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
        public string Email { get; set; }

        [Range(0, 100)] 
        public int? Progression { get; set; } = 50; // FIXME: Magic int is magic. Really the middle progression level.

        [Required]
        public Equipment Equipment { get; set; } = Equipment.None;

        [Required]
        public bool NeedsRest { get; set; }

        [NotMapped]
        public Equipment[] EquipmentBinder { 
            get => Enum.GetValues<Equipment>().Cast<Equipment>().Where(e => Equipment.HasFlag(e)).ToArray();
            set => Equipment = value.Aggregate(Equipment.None, (a, e) => a | e); 
        }
    }
}
