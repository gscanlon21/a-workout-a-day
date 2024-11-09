using Core.Models.Footnote;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Data.Entities.Footnote;

/// <summary>
/// A collection of sage advice.
/// </summary>
[Table("footnote")]
[DebuggerDisplay("{Note} - {Source}")]
public class Footnote : IFootnote
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>
    /// A helpful snippet of fitness advice to show the users.
    /// </summary>
    [Required]
    public string Note { get; init; } = null!;

    /// <summary>
    /// Either a link or a name that was the reference of the note.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Affirmations vs Fitness Tips.
    /// </summary>
    [Required]
    public FootnoteType Type { get; init; }
}
