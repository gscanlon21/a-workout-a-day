﻿using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Web.Views.Index;

namespace Web.Controllers.Newsletter;

[Route("n", Order = 1)]
[Route("newsletter", Order = 2)]
public class NewsletterController : ViewController
{
    private readonly NewsletterRepo _newsletterRepo;

    public NewsletterController(NewsletterRepo newsletterRepo)
    {
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
        //Response.GetTypedHeaders().LastModified = newsletter?.UserWorkout.DateTime;
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
            newsletter.Client = client;
            newsletter.HideFooter = hideFooter;
            return View(nameof(Newsletter), newsletter);
        }

        return NoContent();
    }
}
