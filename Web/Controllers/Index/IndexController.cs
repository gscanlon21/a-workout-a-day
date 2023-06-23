using Core.Models.Options;
using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web.Code.TempData;
using Web.Controllers.User;
using Web.ViewModels.User;

namespace Web.Controllers.Index;

public class IndexController : ViewController
{
    private readonly CoreContext _context;
    private readonly Data.Repos.UserRepo _userService;
    private readonly IOptions<SiteSettings> _siteSettings;

    public IndexController(CoreContext context, Data.Repos.UserRepo userService, IOptions<SiteSettings> siteSettings) : base()
    {
        _siteSettings = siteSettings;
        _context = context;
        _userService = userService;
    }

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

    /// <summary>
    /// Landing page.
    /// </summary>
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
            var newUser = new Data.Entities.User.User(viewModel.Email, viewModel.AcceptedTerms, viewModel.IsNewToFitness);

            try
            {
                // This will set the Id prop on newUser when changes are saved
                _context.Add(newUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException != null && e.InnerException.Message.Contains("duplicate key"))
            {
                // User may have clicked the back button after personalizing their routine right after signing up
                return RedirectToAction(nameof(Index), Name);
            }

            // Need a token for if the user chooses to manage their preferences after signup
            var token = await _userService.AddUserToken(newUser, durationDays: 2);
            TempData[TempData_User.SuccessMessage] = "Thank you for signing up!";
            return RedirectToAction(nameof(UserController.Edit), UserController.Name, new { newUser.Email, token });
            //return View("Create", new UserCreateViewModel(newUser, token) { WasSubscribed = true });
        }

        viewModel.WasSubscribed = false;
        return View("Create", viewModel);
    }

    #region User Validation

    /// <summary>
    /// Validation route for whether a user already exists in the database
    /// </summary>
    [AllowAnonymous, Route("availability")]
    public async Task<JsonResult> IsUserAvailable(string email)
    {
        email = email.Trim();

        // Don't let users signup as our domain.
        if (email.Contains(_siteSettings.Value.Domain, StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult("Invalid email.");
        }

        // Gmail does not support position:absolute.
        // https://www.caniemail.com/search/?s=absolute 
        var validGmailEnding = $"+{_siteSettings.Value.ApexDomainSansTLD}@gmail.com";
        if (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
            && !email.EndsWith(validGmailEnding, StringComparison.OrdinalIgnoreCase))
        {
            var splitEmail = email.Split('+', '@');
            // Fragmented; muddled; diorganized; disjointed; jumbled.
            return new JsonResult($"Gmail is not a supported email client. Emails may appear garbled. If you understand, use the email: {splitEmail[0] + validGmailEnding}");
        }

        // The same user is already signed up.
        if (await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email)))
        {
            return new JsonResult("Invalid email.");
        }

        return new JsonResult(true);
    }

    #endregion
}
