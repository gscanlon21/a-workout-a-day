using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Table(nameof(Intensity)), Comment("Intensity level of an exercise variation")]
    public class Intensity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public Proficiency Proficiency { get; set; } = null!;

        [Required]
        public IntensityLevel IntensityLevel { get; set; }

        [Required]
        public Progression Progression { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Variation.Intensities))]
        public virtual Variation Variation { get; set; } = null!;

        [InverseProperty(nameof(EquipmentGroup.Intensity)), UIHint(nameof(EquipmentGroup))]
        public ICollection<EquipmentGroup> EquipmentGroups { get; set; } = new List<EquipmentGroup>();
    }

    [Owned]
    public record Progression ([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
    {
        public int GetMinOrDefault => Min ?? 0;
        public int GetMaxOrDefault => Max ?? 100;
    }

    public enum IntensityLevel
    {
        Main = 0,
        Stretch = 1,
    }

    [Owned]
    public record Proficiency(int? Reps, int? Secs)
    {
        public int Sets { get; set; }
    }
}
