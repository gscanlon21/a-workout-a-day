using Core.Models.Footnote;

using System.Diagnostics;

namespace Lib.ViewModels.Footnote;

/// <summary>
/// A collection of sage advice.
/// </summary>
[DebuggerDisplay("{Note} - {Source}")]
public class FootnoteViewModel
{
    public int Id { get; init; }

    /// <summary>
    /// A helpful snippet of fitness advice to show the users.
    /// </summary>
    public string Note { get; init; } = null!;

    /// <summary>
    /// Either a link or a name that was the reference of the note.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Affirmations vs Fitness Tips.
    /// </summary>
    public FootnoteType Type { get; init; }
}
