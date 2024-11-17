using Core.Dtos.User;
using Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Lib.Services;

/// <summary>
/// User helpers.
/// </summary>
public class UserService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<SiteSettings> _siteSettings;

    public UserService(IHttpClientFactory httpClientFactory, IOptions<SiteSettings> siteSettings)
    {
        _siteSettings = siteSettings;
        _httpClient = httpClientFactory.CreateClient();
        if (_httpClient.BaseAddress != _siteSettings.Value.ApiUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.ApiUri;
        }
    }

    public async Task<ApiResult<UserNewsletterDto>> GetUser(string email, string token)
    {
        var response = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/User/User?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");
        return await ApiResult<UserNewsletterDto>.FromResponse(response);
    }

    public async Task LogException(string? email, string? token, string? message)
    {
        await _httpClient.PostAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/User/LogException", new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            { "email", email },
            { "token", token },
            { "message", message }
        }));
    }
}

