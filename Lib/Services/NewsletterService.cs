﻿using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.Options;
using Lib.Dtos.Footnote;
using Lib.Dtos.User;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Lib.Services;

internal class NewsletterService
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
    private readonly UserService _userService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<SiteSettings> _siteSettings;

    public NewsletterService(HttpClient httpClient, IOptions<SiteSettings> siteSettings, UserService userService, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
        _siteSettings = siteSettings;
        _httpClient = httpClient;
        if (_httpClient.BaseAddress != _siteSettings.Value.ApiUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.ApiUri;
        }
    }

    public async Task<IList<Footnote>?> GetFootnotes(UserNewsletterViewModel user, int count = 1, FootnoteType ofType = FootnoteType.All)
    {
        return await _httpClient.GetFromJsonAsync<List<Footnote>>($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/GetFootnotes");
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<NewsletterViewModel?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null, Client client = Client.None)
    {
        return await _httpClient.GetFromJsonAsync<NewsletterViewModel>($"{_siteSettings.Value.ApiUri.AbsolutePath}/newsletter/Newsletter?email={email}&token={token}&date={date}&client={client}");
    }
}
