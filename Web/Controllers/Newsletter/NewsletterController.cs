using Core.Models.Newsletter;
using Data.Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User;

namespace Web.Controllers.Newsletter;

[Route("n", Order = 1)]
[Route("newsletter", Order = 2)]
public partial class NewsletterController : ViewController
{
    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Newsletter";

    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UserRepo _userService;
    private readonly NewsletterRepo _newsletterService;

    public NewsletterController(CoreContext context, UserRepo userService, NewsletterRepo newsletterService, IServiceScopeFactory serviceScopeFactory) : base()
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
        _newsletterService = newsletterService;
        _userService = userService;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}/{{date}}", Order = 1)]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 2)]
    [Route("demo", Order = 3)]
    public async Task<IActionResult> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null)
    {
        var newsletter = await _newsletterService.Newsletter(email, token, date ?? Today, Client.Email);
        if (newsletter != null)
        {
            return View(nameof(Newsletter), newsletter);
        }

        return NoContent();
    }
}
