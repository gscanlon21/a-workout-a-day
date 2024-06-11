using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Components.User;

/// <summary>
/// Viewmodel for Confirmation.cshtml
/// </summary>
public class FootnoteViewModel
{
    public string Token { get; init; } = null!;
    public Data.Entities.User.User User { get; init; } = null!;

    [Display(Name = "Custom Footnotes")]
    public IList<Data.Entities.Footnote.UserFootnote> Footnotes { get; init; } = null!;
}
