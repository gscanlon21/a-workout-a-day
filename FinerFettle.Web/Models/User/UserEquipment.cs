using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// Maps a user with their equipment.
    /// </summary>
    [Table("user_equipment")]
    public class UserEquipment
    {
        [ForeignKey(nameof(Exercise.Equipment.Id))]
        public int EquipmentId { get; set; }

        [ForeignKey(nameof(Models.User.User.Id))]
        public int UserId { get; set; }

        [InverseProperty(nameof(Models.User.User.UserEquipments))]
        public virtual User User { get; set; } = null!;

        [InverseProperty(nameof(Exercise.Equipment.UserEquipments))]
        public virtual Equipment Equipment { get; set; } = null!;
    }
}
