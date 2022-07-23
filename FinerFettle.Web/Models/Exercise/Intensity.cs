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
        public Proficiency Proficiency { get; set; } = null!;

        [Required]
        public IntensityLevel IntensityLevel { get; set; }

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        public string GetHarderVariation()
        {
            if (!MuscleContractions.HasFlag(MuscleContractions.Isometric) && Proficiency.Secs != null)
            {
                return $"For a harder workout, add a {Proficiency.Secs} second hold at the peak of each repetition";
            }

            return string.Empty;
        }

        public string GetProficiency()
        {
            if (Proficiency.Sets.HasValue && Proficiency.Reps.HasValue && Proficiency.Secs.HasValue && MuscleContractions.HasFlag(MuscleContractions.Isometric))
            {
                return $"{Proficiency.Sets} set(s) of {Proficiency.Reps} reps with a {Proficiency.Secs} second hold";
            }
            else if (Proficiency.Sets.HasValue && Proficiency.Reps.HasValue)
            {
                return $"{Proficiency.Sets} set(s) of {Proficiency.Reps} reps";
            }
            else if (Proficiency.Sets.HasValue && Proficiency.Secs.HasValue)
            {
                return $"{Proficiency.Sets} set(s) of a {Proficiency.Secs} second hold";
            }
            return String.Empty;
        }
    }

    public enum IntensityLevel
    {
        Main = 0,
        Stretch = 1,
    }

    [Owned]
    public record Proficiency(int? Sets, int? Reps, int? Secs);
}
