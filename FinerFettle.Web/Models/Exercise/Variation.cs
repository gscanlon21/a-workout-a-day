using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Comment("Progressions of an exercise"), Table(nameof(Variation))]
    public class Variation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, 100)]
        public int? Progression { get; set; }

        [Required]
        public string Instruction { get; set; }

        [Required]
        // NOTETOSELF: For weighted exercises, add a new base exercise (sa. weighted squat). And list the variations under that umbrella. 
        // Maybe add a link in the newsletter saying 'Feel good after that workout? Click here for a harder workout tomorrow' that logs the next workout should use weights.
        public Equipment Equipment { get; set; }

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        // TODO: Proficiency class?
        public int? ProficiencySets { get; set; }
        public int? ProficiencyReps { get; set; }
        public int? ProficiencySecs { get; set; }
    }
}
