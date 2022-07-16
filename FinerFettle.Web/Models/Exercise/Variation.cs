using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Workout
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

        [Required, Range(0, 100)]
        public int Progression { get; set; }


        [Required]
        public string Instruction { get; set; }

        // TODO: Proficiency class?
        public int? ProficiencySets { get; set; }
        public int? ProficiencyReps { get; set; }
        public int? ProficiencySecs { get; set; }
    }
}
