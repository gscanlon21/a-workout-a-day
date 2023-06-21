using Data.Data;
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
    private readonly Lib.Services.UserService _userService;
    private readonly Lib.Services.NewsletterService _newsletterService;

    public NewsletterController(CoreContext context, Lib.Services.UserService userService, Lib.Services.NewsletterService newsletterService, IServiceScopeFactory serviceScopeFactory) : base()
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
    public async Task<IActionResult> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null, string? format = null)
    {
        var newsletter = await _newsletterService.Newsletter(email, token, date, format);
        if (newsletter != null)
        {
            return View(nameof(Newsletter), newsletter);
        }

        return NoContent();

        /*
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null || user.Disabled
            // User is a debug user. They should see the DebugNewsletter instead.
            || user.Features.HasFlag(Features.Debug))
        {
            return NoContent();
        }

        if (date.HasValue)
        {
            ViewData[ViewData_Newsletter.NeedsDeload] = false;
            return View(nameof(Newsletter), await _newsletterService.NewsletterOld(user, token, date.Value, format));
        }

        // User was already sent a newsletter today.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any() && n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails))
        {
            return NoContent();
        }

        // User has received an email with a confirmation message, but they did not click to confirm their account.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any()) && user.LastActive == null)
        {
            return NoContent();
        }

        if (user.RestDays.HasFlag(DaysExtensions.FromDate(Today)))
        {
            if (user.SendMobilityWorkouts)
            {
                ViewData[ViewData_Newsletter.NeedsDeload] = false;
                return View("OffDayNewsletter", await _newsletterService.OffDayNewsletter(user, token, format));
            }

            return NoContent();
        }

        ViewData[ViewData_Newsletter.NeedsDeload] = false;
        return View(nameof(Newsletter), await _newsletterService.OnDayNewsletter(user, token, format));
        */
    }
}
