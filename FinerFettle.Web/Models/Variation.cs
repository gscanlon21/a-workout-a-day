using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models
{
    [Comment("Progressions of an exercise")]
    public class Variation
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProficiencySets { get; set; }
        public int ProficiencyReps { get; set; }
        public int Progression { get; set; }
    }
}
