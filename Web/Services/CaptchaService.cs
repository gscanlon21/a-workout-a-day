using Core.Models.Options;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Web.Services;

public class CaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<CaptchaSettings> _captchaSettings;

    public CaptchaService(IHttpClientFactory httpClientFactory, IOptions<CaptchaSettings> captchaSettings)
    {
        _captchaSettings = captchaSettings;
        _httpClient = httpClientFactory.CreateClient();
        if (_httpClient.BaseAddress != _captchaSettings.Value.ApiUri)
        {
            _httpClient.BaseAddress = _captchaSettings.Value.ApiUri;
        }
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<CaptchaResponse?> VerifyCaptcha(string solution)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_captchaSettings.Value.ApiUri.AbsolutePath}", new CaptchaRequest(solution, _captchaSettings.Value.ApiKey));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CaptchaResponse>() : null;
    }
}

public class CaptchaRequest
{
    [SetsRequiredMembers]
    public CaptchaRequest(string solution, string secret)
    {
        Solution = solution;
        Secret = secret;
    }

    public required string Solution { get; set; }
    public required string Secret { get; set; }
    public string? SiteKey { get; set; }
}

public class CaptchaResponse
{
    public bool Success { get; set; }
    public IList<object>? Errors { get; set; }
}
