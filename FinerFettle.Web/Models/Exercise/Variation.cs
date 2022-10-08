using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Progressions of an exercise
    /// </summary>
    [Table(nameof(Variation)), Comment("Progressions of an exercise")]
    [DebuggerDisplay("Code = {Code}")]
    public class Variation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        public string? DisabledReason { get; set; } = null;

        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public ExerciseType ExerciseType { get; set; }

        public SportsFocus SportsFocus { get; set; }

        [InverseProperty(nameof(Intensity.Variation))]
        public ICollection<Intensity> Intensities { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Exercise.Variations))]
        public virtual Exercise Exercise { get; set; } = null!;

        [InverseProperty(nameof(User.UserVariation.Variation))]
        public virtual ICollection<UserVariation> UserVariations { get; set; } = null!;
    }
}
