using ADay.Data;
using Core.Models.User;
using Data.Entities.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.SystemFootnote;

namespace Web.Components.Users;

public class SystemFootnoteViewComponent : ViewComponent
{
    private readonly SharedContext _context;

    public SystemFootnoteViewComponent(SharedContext context)
    {
        _context = context;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "SystemFootnote";

    public async Task<IViewComponentResult> InvokeAsync(User user, string token)
    {
        // Only admins can edit system footnotes.
        if (!user.Features.HasFlag(Features.Admin))
        {
            return Content("");
        }

        var footnotes = await _context.Footnotes.AsNoTracking().TagWithCallSite()
            .Where(f => EmailConsts.FootnoteTypes.Contains(f.Type))
            .OrderBy(f => f.Type)
            .ThenBy(f => f.Note)
            .ToListAsync();

        return View("SystemFootnote", new SystemFootnoteViewModel()
        {
            User = user,
            Token = token,
            Footnotes = footnotes,
        });
    }
}
