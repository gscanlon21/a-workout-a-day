using Lib.Dtos.Newsletter;
using Lib.Dtos.User;
using Lib.Models.Newsletter;
using Core.Models.Exercise;
using Core.Models.Options;
using Core.Models.User;
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

    private const double WeightSecondaryMusclesXTimesLess = 3;

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

    public const int IncrementMuscleTargetBy = 10;

    /// <summary>
    /// The volume each muscle group should be exposed to each week.
    /// 
    /// ~24 per exercise.
    /// 
    /// https://www.bodybuilding.com/content/how-many-exercises-per-muscle-group.html
    /// 50-70 for minor muscle groups.
    /// 90-120 for major muscle groups.
    /// </summary>
    public static readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Abdominals] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Glutes] = 90..170, // Largest muscle group in the body.
        [MuscleGroups.Hamstrings] = 90..150, // Major muscle.
        [MuscleGroups.Quadriceps] = 90..150, // Major muscle.
        [MuscleGroups.Deltoids] = 90..150, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.Pectorals] = 90..150, // Major muscle.
        [MuscleGroups.Trapezius] = 90..150, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 90..150, // Major muscle.
        [MuscleGroups.HipFlexors] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Calves] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Forearms] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Rhomboids] = 50..90, // Minor muscle.
        [MuscleGroups.Biceps] = 50..90, // Minor muscle.
        [MuscleGroups.Triceps] = 50..90, // Minor muscle.
        [MuscleGroups.SerratusAnterior] = 30..70, // Miniature muscle.
        [MuscleGroups.RotatorCuffs] = 30..70, // Miniature muscle.
        [MuscleGroups.HipAdductors] = 30..70, // Miniature muscle.
        [MuscleGroups.TibialisAnterior] = 0..50, // Generally doesn't require strengthening. 
    };

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

