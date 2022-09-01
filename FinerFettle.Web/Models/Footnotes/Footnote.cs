using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Footnotes
{
    /// <summary>
    /// A collection of sage advice.
    /// </summary>
    [Comment("Sage advice"), Table(nameof(Footnote))]
    [DebuggerDisplay("Note = {Note}")]
    public class Footnote
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public string Note { get; set; } = null!;

        // TODO? FootnoteReferences table for sources
    }
}
