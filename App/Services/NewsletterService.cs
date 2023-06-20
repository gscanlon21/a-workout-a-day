using App.Dtos.User;
using App.ViewModels.Newsletter;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace App.Services;

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
    private readonly UserService _userService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterService(HttpClient httpClient, UserService userService, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
        _httpClient = httpClient;
        //_httpClient.BaseAddress = new Uri("https://aworkoutaday.com");
        _httpClient.BaseAddress = new Uri("https://localhost:7107");
    }

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetDebugExercises(User user, string token, int count = 1)
    {
        return await _httpClient.GetFromJsonAsync<List<ExerciseViewModel>>($"/newsletter/GetDebugExercises");
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    //[Route("debug")]
    public async Task<object?> Debug(string email, string token)
    {
        return await _httpClient.GetFromJsonAsync<object>($"/newsletter/Debug");
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<object?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null, string? format = null)
    {
        return await _httpClient.GetFromJsonAsync<object>($"/newsletter/Newsletter");
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    public async Task<NewsletterViewModel?> OnDayNewsletter(User user, string token, string? format)
    {
        return await _httpClient.GetFromJsonAsync<NewsletterViewModel>($"/newsletter/OnDayNewsletter");
    }

    /// <summary>
    /// The mobility/stretch newsletter for days off strength training.
    /// </summary>
    public async Task<OffDayNewsletterViewModel?> OffDayNewsletter(User user, string token, string? format)
    {
        return await _httpClient.GetFromJsonAsync<OffDayNewsletterViewModel>($"/newsletter/OffDayNewsletter");
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter based on a date.
    /// </summary>
    public async Task<NewsletterViewModel?> NewsletterOld(User user, string token, DateOnly date, string? format)
    {
        return await _httpClient.GetFromJsonAsync<NewsletterViewModel>($"/newsletter/NewsletterOld");
    }
}
