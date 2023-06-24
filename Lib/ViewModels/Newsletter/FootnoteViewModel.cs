using Lib.ViewModels.User;

namespace Lib.ViewModels.Newsletter;

public class FootnoteViewModel
{
    public UserNewsletterViewModel User { get; init; } = null!;
    public IList<FootnoteViewModel> Footnotes { get; init; } = null!;
}
