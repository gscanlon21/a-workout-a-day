using Data.Entities.Footnote;
using Data.Models.User;

namespace Data.Models.Newsletter;

public class FootnoteModel
{
    public UserNewsletterModel User { get; init; } = null!;
    public IList<Footnote> Footnotes { get; init; } = null!;
}
