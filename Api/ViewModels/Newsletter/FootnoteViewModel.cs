using Api.ViewModels.User;
using Data.Entities.Footnote;

namespace Api.ViewModels.Newsletter;

public class FootnoteViewModel
{
    public UserNewsletterViewModel User { get; init; } = null!;
    public IList<Footnote> Footnotes { get; init; } = null!;
}
