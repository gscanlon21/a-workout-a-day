using ADay.Core.Models.Footnote;
using ADay.Data.Entities.Footnote;
using System.ComponentModel.DataAnnotations;

namespace Web.Views.Shared.Components.SystemFootnote;

public class SystemFootnoteViewModel
{
    public string Token { get; init; } = null!;
    public Data.Entities.Users.User User { get; init; } = null!;

    [Display(Name = "System Footnotes")]
    public IList<Footnote> Footnotes { get; init; } = null!;

    /// <summary>
    /// Types of footnotes to show to the user.
    /// </summary>
    [Display(Name = "Footnote Type", Description = "")]
    public FootnoteType FootnoteType { get; set; }
}
