using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.Extensions;
using Web.Data;
using Web.Models.Footnote;
using Web.ViewModels.Newsletter;
using Web.ViewModels.User;

namespace Web.Components.Newsletter;

/// <summary>
/// Renders several random footnotes.
/// </summary>
public class FootnoteViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Footnote";

    private readonly CoreContext _context;

    public FootnoteViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(UserNewsletterViewModel user, int count = 1, FootnoteType ofType = FootnoteType.All)
    {
        // Only show the types the user wants to see
        ofType &= user.FootnoteType;

        var footnotes = await _context.Footnotes
            .OrderBy(_ => Guid.NewGuid())
            // Has any flag
            .Where(f => (f.Type & ofType) != 0)
            .Take(count)
            .ToListAsync();

        if (footnotes == null || !footnotes.Any())
        {
            return Content(string.Empty);
        }

        return View("Footnote", new FootnoteViewModel()
        {
            User = user,
            Footnotes = footnotes
        });
    }
}
