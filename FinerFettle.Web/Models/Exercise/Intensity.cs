using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Comment("Intensity level of an exercise variation"), Table(nameof(Intensity))]
    public class Intensity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public IntensityLevel IntensityLevel { get; set; }

        [Required]
        public Proficiency Proficiency { get; set; }
    }

    public enum IntensityLevel
    {
        Main = 0,
        Stretch = 1,
    }

    [Owned]
    public record Proficiency(int? Sets, int? Reps, int? Secs)
    {
        public override string ToString()
        {
            if (Sets.HasValue && Reps.HasValue && Secs.HasValue)
            {
                return $"{Sets} set(s) of {Reps} reps with a {Secs} sec hold";
            }
            else if (Sets.HasValue && Reps.HasValue)
            {
                return $"{Sets} set(s) of {Reps} reps";
            }
            else if (Sets.HasValue && Secs.HasValue)
            {
                return $"{Sets} set(s) of a {Secs} sec hold";
            }
            return String.Empty;
        }
    }
}
