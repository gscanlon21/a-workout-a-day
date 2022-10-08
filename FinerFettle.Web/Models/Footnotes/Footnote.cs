using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Footnotes
{
    /// <summary>
    /// A collection of sage advice.
    /// </summary>
    [Table("footnote"), Comment("Sage advice")]
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
