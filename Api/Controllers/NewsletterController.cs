using Core.Consts;
using Data.Dtos.Newsletter;
using Data.Entities.Footnote;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public partial class NewsletterController(NewsletterRepo newsletterRepo) : ControllerBase
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    protected static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    [HttpGet("Footnotes")]
    public async Task<IList<Footnote>> GetFootnotes(string? email = null, string? token = null, int count = 1)
    {
        return await newsletterRepo.GetFootnotes(email, token, count);
    }

    [HttpGet("Footnotes/Custom")]
    public async Task<IList<UserFootnote>> GetUserFootnotes(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, int count = 1)
    {
        return await newsletterRepo.GetUserFootnotes(email, token, count);
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet("Newsletter")]
    public async Task<NewsletterDto?> GetNewsletter(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, DateOnly? date = null)
    {
        return await newsletterRepo.Newsletter(email, token, date);
    }
}
