using Core.Models.Options;
using Microsoft.Extensions.Options;

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

