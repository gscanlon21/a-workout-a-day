﻿using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Intensity level of an exercise variation
    /// </summary>
    [Table("variation"), Comment("Intensity level of an exercise variation")]
    [DebuggerDisplay("Name = {Name}")]
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
        public Progression Progression { get; set; } = null!;

        [Required]
        public ExerciseType ExerciseType { get; set; }

        public SportsFocus SportsFocus { get; set; }

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        [InverseProperty(nameof(Models.Exercise.Exercise.Variations))]
        public virtual Exercise Exercise { get; set; } = null!;

        [InverseProperty(nameof(EquipmentGroup.Variation)), UIHint(nameof(EquipmentGroup))]
        public ICollection<EquipmentGroup> EquipmentGroups { get; set; } = new List<EquipmentGroup>();

        [InverseProperty(nameof(UserVariation.Variation))]
        public virtual ICollection<UserVariation> UserVariations { get; set; } = null!;

        [InverseProperty(nameof(IntensityPreference.Variation))]
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
