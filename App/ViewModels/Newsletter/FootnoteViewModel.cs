using App.Dtos.Footnote;
using App.ViewModels.User;

namespace App.ViewModels.Newsletter;

public class FootnoteViewModel
{
    public UserNewsletterViewModel User { get; init; } = null!;
    public IList<Footnote> Footnotes { get; init; } = null!;
}
