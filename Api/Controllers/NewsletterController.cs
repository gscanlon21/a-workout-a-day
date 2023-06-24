using Data.Models.Newsletter;
using Data.Models.User;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Data;
using Data.Data.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Repos;
using Core.Models.Footnote;

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
    private readonly UserController _userService;
    private readonly UserRepo _userRepo;
    private readonly NewsletterRepo _newsletterRepo;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterController(NewsletterRepo newsletterRepo, UserRepo userRepo, CoreContext context, UserController userService, IServiceScopeFactory serviceScopeFactory)
    {
        _newsletterRepo = newsletterRepo;
        _userRepo = userRepo;
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
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
    public async Task<NewsletterModel?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null, Client client = Client.None)
    {
        return await _newsletterRepo.Newsletter(email, token, date, client);
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    [HttpGet("Debug")]
    public async Task<DebugModel?> Debug(string email, string token, Client client = Client.None)
    {
        return await _newsletterRepo.Debug(email, token, client);
    }
}
