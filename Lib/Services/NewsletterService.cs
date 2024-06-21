using Core.Consts;
using Core.Dtos.Footnote;
using Core.Models.Options;
using Lib.Pages.Newsletter;
using Lib.Pages.Shared.Newsletter;
using Lib.ViewModels;
using Microsoft.Extensions.Options;

namespace Lib.Services;

public class NewsletterService
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    protected static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    private readonly HttpClient _httpClient;
    private readonly IOptions<SiteSettings> _siteSettings;

    public NewsletterService(IHttpClientFactory httpClientFactory, IOptions<SiteSettings> siteSettings)
    {
        _siteSettings = siteSettings;
        _httpClient = httpClientFactory.CreateClient();
        if (_httpClient.BaseAddress != _siteSettings.Value.ApiUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.ApiUri;
        }
    }

    public async Task<ApiResult<IList<FootnoteDto>>> GetFootnotes(UserNewsletterViewModel? user = null, int count = 1)
    {
        if (user == null)
        {
            var response = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/Footnotes?count={count}");
            return await ApiResult<IList<FootnoteDto>>.FromResponse(response);
        }

        var userResponse = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/Footnotes?count={count}&email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(user.Token)}");
        return await ApiResult<IList<FootnoteDto>>.FromResponse(userResponse);
    }

    public async Task<ApiResult<IList<FootnoteDto>>> GetUserFootnotes(UserNewsletterViewModel user, int count = 1)
    {
        var response = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/Footnotes/Custom?count={count}&email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(user.Token)}");
        return await ApiResult<IList<FootnoteDto>>.FromResponse(response);
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<ApiResult<NewsletterViewModel>> Newsletter(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, DateOnly? date = null)
    {
        var response = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/Newsletter?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&date={date}");
        return await ApiResult<NewsletterViewModel>.FromResponse(response);
    }
}
