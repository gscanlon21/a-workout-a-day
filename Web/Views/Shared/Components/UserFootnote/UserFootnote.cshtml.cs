using System.ComponentModel.DataAnnotations;

namespace Web.Views.Shared.Components.UserFootnote;

public class UserFootnoteViewModel
{
    public string Token { get; init; } = null!;
    public Data.Entities.Users.User User { get; init; } = null!;

    [Display(Name = "Custom Footnotes")]
    public IList<Data.Entities.Footnote.UserFootnote> Footnotes { get; init; } = null!;
}
