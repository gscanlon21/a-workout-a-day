using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Components.Newsletter;

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

    public async Task<IViewComponentResult> InvokeAsync(int count = 1)
    {
        var footnote = await _context.Footnotes.OrderBy(_ => Guid.NewGuid()).Take(count).ToListAsync();
        if (footnote == null)
        {
            return Content(string.Empty);
        }

        return View("Footnote", footnote);
    }
}
