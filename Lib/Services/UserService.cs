using Core.Models.Options;
using Lib.ViewModels.Newsletter;
using Lib.ViewModels.User;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

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

    public async Task<UserNewsletterViewModel?> GetUser(string email, string token)
    {
        var response = await _httpClient.GetAsync($"{_siteSettings.Value.ApiUri.AbsolutePath}/User/User?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }
        else if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserNewsletterViewModel>();
        }

        return null;
    }

    public async Task<IList<UserWorkoutViewModel>?> GetWorkouts(string email, string token)
    {
        return await _httpClient.GetFromJsonAsync<List<UserWorkoutViewModel>>($"{_siteSettings.Value.ApiUri.AbsolutePath}/User/Workouts?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}");
    }
}

