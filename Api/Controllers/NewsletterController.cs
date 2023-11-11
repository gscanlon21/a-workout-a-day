using Core.Consts;
using Core.Models.Footnote;
using Data.Dtos.Newsletter;
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

    [HttpGet("GetFootnotes")]
    public async Task<IList<Data.Entities.Footnote.Footnote>> GetFootnotes(string? email, string? token, int count = 1, FootnoteType ofType = FootnoteType.Bottom)
    {
        return await newsletterRepo.GetFootnotes(email, token, count, ofType);
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
