using ADay.Core.Models.Footnote;
using ADay.Data;
using ADay.Data.Entities.Footnote;
using Data;
using Data.Repos;
using Lib.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.TempData;
using Web.Controllers.Users;
using Web.Views.Shared.Components.SystemFootnote;

namespace Web.Controllers.Footnotes;


[Route($"f", Order = 1)]
[Route($"footnote", Order = 2)]
public partial class FootnoteController : ViewController
{
    private readonly SharedContext _sharedContext;
    private readonly UserRepo _userRepo;

    public FootnoteController(SharedContext sharedContext, UserRepo userRepo)
    {
        _sharedContext = sharedContext;
        _userRepo = userRepo;
    }

    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Footnote";


    [HttpPost, Route("footnote/add")]
    public async Task<IActionResult> AddFootnote(string email, string token, [FromForm] string note, [FromForm] string? source, SystemFootnoteViewModel viewModel)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        if (viewModel.FootnoteType == FootnoteType.None || viewModel.FootnoteType == FootnoteType.Custom)
        {
            TempData[TempData_User.FailureMessage] = $"Invalid footnote type: {viewModel.FootnoteType}";
            return RedirectToAction(nameof(UserController.Edit), new { email, token });
        }

        _sharedContext.Footnotes.Add(new Footnote()
        {
            Note = note,
            Source = source,
            Type = viewModel.FootnoteType
        });

        await _sharedContext.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost, Route("footnote/remove")]
    public async Task<IActionResult> RemoveFootnote(string email, string token, [FromForm] int footnoteId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await _sharedContext.Footnotes
            .Where(f => f.Id == footnoteId)
            .ExecuteDeleteAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }
}
