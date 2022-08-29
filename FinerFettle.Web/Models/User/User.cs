using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// User who signed up for the newsletter.
    /// </summary>
    [Comment("User who signed up for the newsletter"), Table(nameof(User))]
    [Index(nameof(Email), IsUnique = true)]
    [DebuggerDisplay("Email = {Email}, Disabled = {Disabled}")]
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public bool OverMinimumAge { get; set; }

        [Required]
        public bool Disabled { get; set; }

        [Required]
        public bool NeedsRest { get; set; }

        [Required]
        public RestDays RestDays { get; set; } = RestDays.None;

        [Required]
        public StrengtheningPreference StrengtheningPreference { get; set; } = StrengtheningPreference.Obtain;

        [Required]
        public Verbosity EmailVerbosity { get; set; } = Verbosity.Normal;

        [Required]
        public ICollection<EquipmentUser> EquipmentUsers { get; set; } = new List<EquipmentUser>();

        // TODO? Allow the user to filter certain exercises out?
        [InverseProperty(nameof(ExerciseUserProgression.User))]
        public virtual ICollection<ExerciseUserProgression> ExerciseProgressions { get; set; } = default!;

        [NotMapped]
        public IEnumerable<int> EquipmentIds => EquipmentUsers.Select(e => e.EquipmentId) ?? new List<int>();

        [NotMapped]
        public double AverageProgression => ExerciseProgressions.Any() ? ExerciseProgressions.Average(p => p.Progression) : 50;

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
    }

    /// <summary>
    /// Maps a user with their equipment.
    /// </summary>
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
