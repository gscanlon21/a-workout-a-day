using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Web.Models.Footnote;

namespace Web.Entities.Footnote;

/// <summary>
/// A collection of sage advice.
/// </summary>
[Table("footnote"), Comment("Sage advice")]
[DebuggerDisplay("{Note} - {Source}")]
public class Footnote
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// A helpful snippet of fitness advice to show the users.
    /// </summary>
    [Required]
    public string Note { get; private init; } = null!;

    /// <summary>
    /// Either a link or a name that was the reference of the note.
    /// </summary>
    public string? Source { get; private init; }

    /// <summary>
    /// Affirmations vs Fitness Tips.
    /// </summary>
    [Required]
    public FootnoteType Type { get; private init; }
}
