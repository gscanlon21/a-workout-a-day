using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Intensity level of an exercise variation
    /// </summary>
    [Table(nameof(Intensity)), Comment("Intensity level of an exercise variation")]
    [DebuggerDisplay("Name = {Name}")]
    public class Intensity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        public string? DisabledReason { get; set; } = null;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public Proficiency Proficiency { get; set; } = null!;

        [Required]
        public Progression Progression { get; set; } = null!;

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        [InverseProperty(nameof(Models.Exercise.Variation.Intensities))]
        public virtual Variation Variation { get; set; } = null!;

        [InverseProperty(nameof(EquipmentGroup.Intensity)), UIHint(nameof(EquipmentGroup))]
        public ICollection<EquipmentGroup> EquipmentGroups { get; set; } = new List<EquipmentGroup>();

        [InverseProperty(nameof(User.UserIntensity.Intensity))]
        public virtual ICollection<UserIntensity> UserIntensities { get; set; } = null!;

        [InverseProperty(nameof(IntensityPreference.Intensity))]
        public ICollection<IntensityPreference> IntensityPreferences { get; set; } = null!;
    }

    /// <summary>
    /// The range of progressions an exercise is available for.
    /// </summary>
    [Owned]
    public record Progression ([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
    {
        public int GetMinOrDefault => Min ?? 0;
        public int GetMaxOrDefault => Max ?? 100;
    }
}
