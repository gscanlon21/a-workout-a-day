using FinerFettle.Web.Entities.Equipment;
using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.Exercise
{
    /// <summary>
    /// Intensity level of an exercise variation
    /// </summary>
    [Table("variation"), Comment("Variations of exercises")]
    [DebuggerDisplay("{Name,nq}")]
    public class Variation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private init; }

        [Required]
        public string Name { get; private init; } = null!;

        [Required]
        public string Code { get; private init; } = null!;

        [Required]
        public ExerciseType ExerciseType { get; private init; }

        [Required]
        public MuscleContractions MuscleContractions { get; private init; }

        /// <summary>
        /// Primary muscles (usually strengthening) worked by the exercise
        /// </summary>
        [Required]
        public MuscleGroups PrimaryMuscles { get; private init; }

        /// <summary>
        /// Secondary (usually stabilizing) muscles worked by the exercise
        /// </summary>
        [Required]
        public MuscleGroups SecondaryMuscles { get; private init; }

        public string? DisabledReason { get; private init; } = null;

        [NotMapped]
        public MuscleGroups AllMuscles => PrimaryMuscles | SecondaryMuscles;

        [InverseProperty(nameof(EquipmentGroup.Variation)), UIHint(nameof(EquipmentGroup))]
        public virtual ICollection<EquipmentGroup> EquipmentGroups { get; private init; } = new List<EquipmentGroup>();

        [InverseProperty(nameof(UserVariation.Variation))]
        public virtual ICollection<UserVariation> UserVariations { get; private init; } = null!;

        [InverseProperty(nameof(Intensity.Variation))]
        public virtual ICollection<Intensity> Intensities { get; private init; } = null!;

        [InverseProperty(nameof(ExerciseVariation.Variation))]
        public virtual ICollection<ExerciseVariation> ExerciseVariations { get; private init; } = null!;
    }
}
