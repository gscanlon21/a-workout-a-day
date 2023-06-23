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
public class UserService
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

    /// <summary>
    /// Grab a user from the db with a specific token
    /// </summary>
    public async Task<User?> GetUser(string email, string token,
        bool includeUserEquipments = false,
        bool includeUserExerciseVariations = false,
        bool includeExerciseVariations = false,
        bool includeMuscles = false,
        bool includeFrequencies = false,
        bool allowDemoUser = false)
    {
        return await _httpClient.GetFromJsonAsync<User>($"{_siteSettings.Value.ApiUri.AbsolutePath}/User/GetUser?Email={email}&Token={token}&includeUserEquipments={includeUserEquipments}&includeUserExerciseVariations={includeUserExerciseVariations}&includeExerciseVariations={includeExerciseVariations}&includeMuscles={includeMuscles}&includeFrequencies={includeFrequencies}&allowDemoUser={allowDemoUser}");
    }

    public async Task<string> AddUserToken(User user, int durationDays = 2)
    {
        return await _httpClient.GetFromJsonAsync<string>($"{_siteSettings.Value.ApiUri.AbsolutePath}/user/AddUserToken");
    }

    /// <summary>
    /// Get the user's weekly training volume for each muscle group.
    /// 
    /// Returns `null` when the user is new to fitness.
    /// </summary>
    public async Task<IDictionary<MuscleGroups, int?>?> GetWeeklyMuscleVolume(User user, int weeks)
    {
        if (weeks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weeks));
        }

        if (user.IsNewToFitness || user.Features.HasFlag(Features.Demo))
        {
            // Feature is disabled in the demo.
            // Feature is disabled for users who are new to fitness because they should be more concerned with working out consistently
            // ... and otherwise when you transition from is-new to is-not-new you would get an increased number of accessory exercises
            // ... from trying to try and hit muscle targets for minor muscles that is-new/functional-exercises don't really target.
            return null;
        }

        return await _httpClient.GetFromJsonAsync<Dictionary<MuscleGroups, int?>?>($"{_siteSettings.Value.ApiUri.AbsolutePath}/user/GetWeeklyMuscleVolume");
    }

    /// <summary>
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    public async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        return await _httpClient.GetFromJsonAsync<(bool needsDeload, TimeSpan timeUntilDeload)>($"{_siteSettings.Value.ApiUri.AbsolutePath}/user/CheckNewsletterDeloadStatus");
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    public async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user)
    {
        return (await GetCurrentAndUpcomingRotations(user)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    public async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user, Frequency frequency)
    {
        return (await GetCurrentAndUpcomingRotations(user, frequency)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    public async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user)
    {
        return await GetCurrentAndUpcomingRotations(user, user.Frequency);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    public async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user, Frequency frequency)
    {
        return await _httpClient.GetFromJsonAsync<NewsletterTypeGroups>($"{_siteSettings.Value.ApiUri.AbsolutePath}/user/GetCurrentAndUpcomingRotations");
    }
}

