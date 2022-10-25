using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.Footnote
{
    /// <summary>
    /// A collection of sage advice.
    /// </summary>
    [Table("footnote"), Comment("Sage advice")]
    [DebuggerDisplay("Note = {Note}")]
    public class Footnote
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private init; }

        [Required]
        public string Note { get; private init; } = null!;

        // TODO? FootnoteReferences table for sources
    }
}
