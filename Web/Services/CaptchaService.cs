using Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Web.Services;

public class CaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CaptchaService> _logger;
    private readonly IOptions<CaptchaSettings> _captchaSettings;

    public CaptchaService(IHttpClientFactory httpClientFactory, IOptions<CaptchaSettings> captchaSettings, ILogger<CaptchaService> logger)
    {
        _logger = logger;
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
        if (_captchaSettings.Value.Enabled == false)
        {
            _logger.Log(LogLevel.Warning, "Skipping captcha - feature is disabled.");
            return new CaptchaResponse()
            {
                Success = true
            };
        }

        var response = await _httpClient.PostAsJsonAsync($"{_captchaSettings.Value.ApiUri.AbsolutePath}", new CaptchaRequest(solution, _captchaSettings.Value.ApiKey));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CaptchaResponse>() : null;
    }

    [method: SetsRequiredMembers]
    private class CaptchaRequest(string solution, string secret)
    {
        public required string Solution { get; init; } = solution;
        public required string Secret { get; init; } = secret;
        public string? SiteKey { get; init; }
    }
}

public class CaptchaResponse
{
    public bool Success { get; init; }
    public IList<object>? Errors { get; init; }
}
