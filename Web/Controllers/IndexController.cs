using Web.Data;
using Web.Entities.User;
using Web.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

public class IndexController : BaseController
{
    public IndexController(CoreContext context) : base(context) { }

    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Index";

    /// <summary>
    /// Server availability check
    /// </summary>
    [Route("ping")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }

    [Route("")]
    public IActionResult Index(bool? wasUnsubscribed = null)
    {
        return View("Create", new UserCreateViewModel()
        {
            WasUnsubscribed = wasUnsubscribed
        });
    }

    [Route(""), HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Email,AcceptedTerms,IsNewToFitness,IExist")] UserCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            // User
            var newUser = new User(viewModel.Email, viewModel.AcceptedTerms, viewModel.IsNewToFitness);

            // This set's the Id prop on newUser
            _context.Add(newUser);

            try
            {
                // Sets the indentity value (Id) of newUser
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException != null && e.InnerException.Message.Contains("duplicate key"))
            {
                return RedirectToAction(nameof(Index), Name);
            }

            // Need a token for if the user chooses to manage their preferences after signup
            var token = new UserToken(newUser.Id) 
            { 
                Expires = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2) 
            };
            newUser.UserTokens.Add(token);
            await _context.SaveChangesAsync();

            return View("Create", new UserCreateViewModel(newUser, token.Token) { WasSubscribed = true });
        }

        viewModel.WasSubscribed = false;
        return View(viewModel);
    }
}
