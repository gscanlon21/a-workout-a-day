using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Models
{
    [Comment("Exercises listed on the website")]
    public class Exercise
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<Variation> Variations { get; set; } = default!;
    }
}
