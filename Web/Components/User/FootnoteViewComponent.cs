using Core.Models.Footnote;
using Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class FootnoteViewComponent(CoreContext context, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Footnote";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (!user.FootnoteType.HasFlag(FootnoteType.Custom))
        {
            return Content("");
        }

        var userFootnotes = await context.Footnotes
            .Where(f => f.UserId == user.Id)
            .OrderBy(f => f.Note)
            .ToListAsync();

        return View("Footnote", new FootnoteViewModel()
        {
            User = user,
            Footnotes = userFootnotes,
            Token = await userRepo.AddUserToken(user, durationDays: 1),
        });
    }
}
