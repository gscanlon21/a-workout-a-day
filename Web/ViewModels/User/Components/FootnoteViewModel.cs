namespace Web.ViewModels.User.Components;

/// <summary>
/// Viewmodel for Confirmation.cshtml
/// </summary>
public class FootnoteViewModel
{
    public string Token { get; init; } = null!;
    public Data.Entities.User.User User { get; init; } = null!;
    public IList<Data.Entities.Footnote.Footnote> Footnotes { get; init; } = null!;
}
