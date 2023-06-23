using Core.Models.Exercise;
using Core.Models.Options;
using Core.Models.User;
using Lib.Dtos.Newsletter;
using Lib.Dtos.User;
using Lib.Models.Newsletter;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Lib.Services;

/// <summary>
/// User helpers.
/// </summary>
internal class UserService
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly HttpClient _httpClient;
    private readonly IOptions<SiteSettings> _siteSettings;

    public UserService(HttpClient httpClient, IOptions<SiteSettings> siteSettings)
    {
        _siteSettings = siteSettings;
        _httpClient = httpClient;
        if (_httpClient.BaseAddress != _siteSettings.Value.ApiUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.ApiUri;
        }
    }
}

