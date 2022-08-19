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

        // TODO: Progressions for each exercise type. Flexibility/Strength/Stability/Cardio progressions
        [Range(0, 100)] 
        public int? Progression { get; set; } = 50; // FIXME: Magic int is magic. Really the middle progression level.

        [Required]
        public bool NeedsRest { get; set; }

        [Required]
        public bool OverMinimumAge { get; set; }

        [Required]
        public bool Disabled { get; set; }

        [Required]
        public ICollection<EquipmentUser> EquipmentUsers { get; set; } = new List<EquipmentUser>();

        [Required]
        public RestDays RestDays { get; set; }

        [Required]
        public StrengtheningPreference StrengtheningPreference { get; set; }

        //[Required]
        //public bool PrefersEccentricExercises { get; set; }

        //[Required]
        //public bool PrefersWeightedExercises { get; set; }

        //[Required]
        //public MuscleGroups StrengthMuscles { get; set; }

        //[Required]
        //public MuscleGroups RecoveryMuscles { get; set; }

        //[Required]
        //public MuscleGroups MobilityMuscles { get; set; }

        // TODO? Many to many relationship with Exercise so user can filter certain exercises out
    }

    public class EquipmentUser
    {
        [ForeignKey(nameof(Exercise.Equipment.Id))]
        public int EquipmentId { get; set; }

        [ForeignKey(nameof(Models.User.User.Id))]
        public int UserId { get; set; }

        [InverseProperty(nameof(Models.User.User.EquipmentUsers))]
        public virtual User User { get; set; } = null!;

        [InverseProperty(nameof(Exercise.Equipment.EquipmentUsers))]
        public virtual Equipment Equipment { get; set; } = null!;
    }
}
