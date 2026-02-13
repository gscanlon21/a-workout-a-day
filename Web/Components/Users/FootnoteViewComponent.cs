using ADay.Core.Models.Footnote;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.Footnote;

namespace Web.Components.Users;

public class FootnoteViewComponent : ViewComponent
{
    private readonly CoreContext _context;

    public FootnoteViewComponent(CoreContext context)
    {
        _context = context;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Footnote";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        // Custom footnotes must be enabled in the user edit form to show in the newsletter.
        if (!user.FootnoteType.HasFlag(FootnoteType.Custom))
        {
            return Content("");
        }

        var userFootnotes = await _context.UserFootnotes
            .Where(f => f.UserId == user.Id)
            .OrderBy(f => f.Note)
            .ToListAsync();

        return View("Footnote", new FootnoteViewModel()
        {
            User = user,
            Token = token,
            Footnotes = userFootnotes,
        });
    }
}
