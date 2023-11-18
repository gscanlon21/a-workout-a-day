using Core.Models.Footnote;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Footnote;

public interface IFootnote
{
    int Id { get; init; }

    /// <summary>
    /// A helpful snippet of fitness advice to show the users.
    /// </summary>
    [Required]
    string Note { get; init; }

    /// <summary>
    /// Either a link or a name that was the reference of the note.
    /// </summary>
    string? Source { get; init; }

    /// <summary>
    /// Affirmations vs Fitness Tips.
    /// </summary>
    FootnoteType Type { get; init; }
}
