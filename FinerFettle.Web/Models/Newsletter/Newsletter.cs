using FinerFettle.Web.Models.Workout;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Newsletter
{
    [Comment("A day's workout routine"), Table(nameof(Newsletter))]
    public class Newsletter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateOnly Date { get; set; }
        
        // FIXME: This is adding a NewsletterId column on the Exercise table. Really, I need a history of what exercises/muscles were previously worked so I can vary them up.
        //public IList<Exercise> Exercises { get; set; }
    }
}
