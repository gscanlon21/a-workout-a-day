using Core.Consts;
using Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Web.Code;
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

    private readonly NewsletterRepo _newsletterService;

    public NewsletterController(NewsletterRepo newsletterService) : base()
    {
        _newsletterService = newsletterService;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}/{{date}}", Order = 1)]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 2)]
    [Route("demo", Order = 3)]
    public async Task<IActionResult> Newsletter(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, DateOnly? date = null)
    {
        //Response.GetTypedHeaders().LastModified = newsletter?.UserWorkout.DateTime;
        Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            // Breaks the contact-us link: https://developers.cloudflare.com/support/more-dashboard-apps/cloudflare-scrape-shield/what-is-email-address-obfuscation/
            NoTransform = true,
            // NoCache would be better for when an old newsletter exists. Just wanting the demo to always receive fresh content for now.
            NoStore = true,
        };

        var newsletter = (await _newsletterService.Newsletter(email, token, date ?? Today))?.AsType<Lib.ViewModels.Newsletter.NewsletterViewModel, Data.Dtos.Newsletter.NewsletterDto>();
        if (newsletter != null)
        {
            return View(nameof(Newsletter), newsletter);
        }

        return NoContent();
    }
}
