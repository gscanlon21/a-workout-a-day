using Core.Models.User;
using Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Web.Views.Index;

namespace Web.Controllers.Newsletter;

[Route("n", Order = 1)]
[Route("newsletter", Order = 2)]
public class NewsletterController : ViewController
{
    private readonly UserRepo _userRepo;
    private readonly CoreContext _context;
    private readonly NewsletterRepo _newsletterRepo;

    public NewsletterController(NewsletterRepo newsletterRepo, UserRepo userRepo, CoreContext context)
    {
        _context = context;
        _userRepo = userRepo;
        _newsletterRepo = newsletterRepo;
    }

    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Newsletter";

    /// <summary>
    /// Root route for building out the workout routine newsletter.
    /// </summary>
    [HttpGet]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}/{{id:int}}", Order = 1)]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}/{{date}}", Order = 2)]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 3)]
    [Route("demo", Order = 4)]
    public async Task<IActionResult> Newsletter(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, DateOnly? date = null, int? id = null, Client client = Client.Web, bool hideFooter = false)
    {
        Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            // Breaks the contact-us link: https://developers.cloudflare.com/support/more-dashboard-apps/cloudflare-scrape-shield/what-is-email-address-obfuscation/
            NoTransform = true,
            // NoCache would be better for when an old newsletter exists. Just wanting the demo to always receive fresh content for now.
            NoStore = true,
        };

        var newsletter = await _newsletterRepo.Newsletter(email, token, date, id);
        if (newsletter != null)
        {
            // NoCache would be better for when an old newsletter exists. Just wanting the demo to always receive fresh content for now.
            Response.GetTypedHeaders().LastModified = new DateTimeOffset(newsletter.UserWorkout.Date, TimeOnly.MinValue, TimeSpan.Zero);

            newsletter.Client = client;
            newsletter.HideFooter = hideFooter;
            return View(nameof(Newsletter), newsletter);
        }

        return NoContent();
    }

    /// <summary>
    /// Root route for building out the workout routine newsletter.
    /// </summary>
    [HttpGet, Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}/test")]
    public async Task<IActionResult> TestNewsletter(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, DateOnly? date = null, int? id = null, Client client = Client.Web, bool hideFooter = false)
    {
        date = DateHelpers.Today.AddMonths(-UserConsts.DeleteWorkoutsAfterXMonths);
        var user = await _userRepo.GetUserStrict(email, token);
        if (!user.Features.HasFlag(Features.Admin))
        {
            return Unauthorized();
        }

        //Response.GetTypedHeaders().LastModified = newsletter?.UserWorkout.DateTime;
        Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            // Breaks the contact-us link: https://developers.cloudflare.com/support/more-dashboard-apps/cloudflare-scrape-shield/what-is-email-address-obfuscation/
            NoTransform = true,
            // NoCache would be better for when an old newsletter exists. Just wanting the demo to always receive fresh content for now.
            NoStore = true,
        };

        // Delete the old workout on the test date so that we can re-create it.
        await _context.UserWorkouts.Where(uw => uw.UserId == user.Id).Where(uw => uw.Date == date).ExecuteDeleteAsync();
        var newsletter = await _newsletterRepo.Newsletter(email, token, date, id);
        if (newsletter != null)
        {
            // NoCache would be better for when an old newsletter exists. Just wanting the demo to always receive fresh content for now.
            Response.GetTypedHeaders().LastModified = new DateTimeOffset(newsletter.UserWorkout.Date, TimeOnly.MinValue, TimeSpan.Zero);

            newsletter.Client = client;
            newsletter.HideFooter = hideFooter;
            return View(nameof(Newsletter), newsletter);
        }

        return NoContent();
    }
}
