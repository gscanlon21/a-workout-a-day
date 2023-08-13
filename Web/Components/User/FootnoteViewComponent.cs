using Core.Models.Footnote;
using Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class FootnoteViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Footnote";

    private readonly CoreContext _context;
    private readonly UserRepo _userRepo;

    public FootnoteViewComponent(CoreContext context, UserRepo userRepo)
    {
        _userRepo = userRepo;
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (!user.FootnoteType.HasFlag(FootnoteType.Custom))
        {
            return Content("");
        }

        var userFootnotes = await _context.Footnotes
            .Where(f => f.UserId == user.Id)
            .OrderBy(f => f.Note)
            .ToListAsync();

        return View("Footnote", new FootnoteViewModel()
        {
            User = user,
            Footnotes = userFootnotes,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
        });
    }
}
