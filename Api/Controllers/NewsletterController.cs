using Core.Models.Footnote;
using Data.Data;
using Data.Models.Newsletter;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public partial class NewsletterController : ControllerBase
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    protected static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    private readonly CoreContext _context;
    private readonly UserController _userController;
    private readonly UserRepo _userRepo;
    private readonly NewsletterRepo _newsletterRepo;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterController(NewsletterRepo newsletterRepo, UserRepo userRepo, CoreContext context, UserController userController, IServiceScopeFactory serviceScopeFactory)
    {
        _newsletterRepo = newsletterRepo;
        _userRepo = userRepo;
        _serviceScopeFactory = serviceScopeFactory;
        _userController = userController;
        _context = context;
    }

    [HttpGet("GetFootnotes")]
    public async Task<IList<Data.Entities.Footnote.Footnote>> GetFootnotes(int count = 1, FootnoteType ofType = FootnoteType.Bottom)
    {
        return await _newsletterRepo.GetFootnotes(count, ofType);
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet("Newsletter")]
    public async Task<NewsletterModel?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null)
    {
        return await _newsletterRepo.Newsletter(email, token, date);
    }
}
