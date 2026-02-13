using ADay.Core.Models.Footnote;
using Data.Entities.Footnote;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.TempData;

namespace Web.Controllers.Users;

public partial class UserController
{
    [HttpPost, Route("footnote/add")]
    public async Task<IActionResult> AddFootnote(string email, string token, [FromForm] string note, [FromForm] string? source)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        _context.Add(new UserFootnote()
        {
            User = user,
            Note = note,
            Source = source,
            Type = FootnoteType.Custom
        });

        await _context.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(Edit), new { email, token });
    }

    [HttpPost, Route("footnote/remove")]
    public async Task<IActionResult> RemoveFootnote(string email, string token, [FromForm] int footnoteId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await _context.UserFootnotes
            // The user has control of this footnote and is not a built-in footnote.
            .Where(f => f.UserId == user.Id)
            .Where(f => f.Id == footnoteId)
            .ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(Edit), new { email, token });
    }
}
