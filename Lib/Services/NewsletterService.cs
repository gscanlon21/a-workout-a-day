using Core.Consts;
using Core.Models.Footnote;
using Core.Models.Options;
using Lib.ViewModels.Footnote;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

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

    public async Task<IList<FootnoteViewModel>?> GetFootnotes(UserNewsletterViewModel? user, int count = 1, FootnoteType ofType = FootnoteType.All)
    {
        // Only show the types the user wants to see
        if (user != null)
        {
            ofType &= user.FootnoteType;
            return await _httpClient.GetFromJsonAsync<List<FootnoteViewModel>>($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/GetFootnotes?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(user.Token)}&count={count}&ofType={ofType}");
        }

        return await _httpClient.GetFromJsonAsync<List<FootnoteViewModel>>($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/GetFootnotes?count={count}&ofType={ofType}");
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<NewsletterViewModel?> Newsletter(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, DateOnly? date = null)
    {
        var response = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/Newsletter?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}&date={date}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }
        else if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NewsletterViewModel>();
        }

        return null;
    }
}
