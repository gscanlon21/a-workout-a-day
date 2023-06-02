using Web.Entities.Footnote;
using Web.ViewModels.User;

namespace Web.ViewModels.Newsletter;

public class FootnoteViewModel
{
    public UserNewsletterViewModel User { get; init; } = null!;
    public IList<Footnote> Footnotes { get; init; } = null!;
}
