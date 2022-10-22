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
    [Table("variation"), Comment("Variations of exercises")]
    [DebuggerDisplay("{Name,nq}")]
    public class Variation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        public string? DisabledReason { get; set; } = null;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public ExerciseType ExerciseType { get; set; }

        [Required]
        public SportsFocus SportsFocus { get; set; }

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        [InverseProperty(nameof(EquipmentGroup.Variation)), UIHint(nameof(EquipmentGroup))]
        public ICollection<EquipmentGroup> EquipmentGroups { get; set; } = new List<EquipmentGroup>();

        [InverseProperty(nameof(UserVariation.Variation))]
        public virtual ICollection<UserVariation> UserVariations { get; set; } = null!;

        [InverseProperty(nameof(Intensity.Variation))]
        public ICollection<Intensity> Intensities { get; set; } = null!;

        [InverseProperty(nameof(ExerciseVariation.Variation))]
        public virtual ICollection<ExerciseVariation> ExerciseVariations { get; set; } = null!;
    }
}
