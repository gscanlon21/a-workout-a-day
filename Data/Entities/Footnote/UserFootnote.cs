using Core.Models.Footnote;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Footnote;

/// <summary>
/// A collection of sage advice.
/// </summary>
[Table("user_footnote"), Comment("Sage advice")]
[DebuggerDisplay("{Note} - {Source}")]
public class UserFootnote : IFootnote
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int UserId { get; init; }

    public DateOnly UserLastSeen { get; set; }

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

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserFootnotes))]
    public User.User User { get; init; } = null!;
}
