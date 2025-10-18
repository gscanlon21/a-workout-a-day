using Core.Code.Exceptions;
using Core.Dtos.Newsletter;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Data.Repos;

/// <summary>
/// User helpers.
/// </summary>
public class UserRepo
{
    private readonly CoreContext _context;

    public UserRepo(CoreContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Grab a user from the db with a specific token.
    /// Throws an exception if the user cannot be found.
    /// </summary>
    public async Task<User> GetUserStrict(string? email, string? token,
        bool includeUserExerciseVariations = false,
        bool includeMuscles = false,
        bool includeFrequencies = false,
        bool allowDemoUser = false)
    {
        return await GetUser(email, token,
            includeMuscles: includeMuscles,
            includeFrequencies: includeFrequencies,
            includeUserExerciseVariations: includeUserExerciseVariations,
            allowDemoUser: allowDemoUser) ?? throw new UserException("User is null.");
    }

    /// <summary>
    /// Grab a user from the db with a specific token.
    /// </summary>
    public async Task<User?> GetUser(string? email, string? token,
        bool includeUserExerciseVariations = false,
        bool includeFrequencies = false,
        bool includeMuscles = false,
        bool allowDemoUser = false)
    {
        if (email == null || token == null)
        {
            return null;
        }

        IQueryable<User> query = _context.Users.AsSplitQuery().TagWithCallSite();

        if (includeMuscles)
        {
            query = query.Include(u => u.UserPrehabSkills);
            query = query.Include(u => u.UserMuscleStrengths);
            query = query.Include(u => u.UserMuscleMobilities);
            query = query.Include(u => u.UserMuscleFlexibilities);
        }

        if (includeFrequencies)
        {
            query = query.Include(u => u.UserFrequencies);
        }

        if (includeUserExerciseVariations)
        {
            query = query.Include(u => u.UserExercises).Include(u => u.UserVariations);
        }

        var user = await query
            // User token is valid.
            .Where(u => u.UserTokens.Any(ut => ut.Token == token && ut.Expires > DateTime.UtcNow))
            .FirstOrDefaultAsync(u => u.Email == email);

        if (!allowDemoUser && user?.IsDemoUser == true)
        {
            throw new UserException("User not authorized.");
        }

        if (user?.Features.HasFlag(Features.Debug) == true)
        {
            user.Verbosity = Verbosity.Debug;
        }

        return user;
    }

    private static string CreateToken(int count = 24) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));
    public async Task<string> AddUserToken(User user, int durationDays = 1) => await AddUserToken(user, DateTime.UtcNow.AddDays(durationDays));
    public async Task<string> AddUserToken(User user, DateTime expires)
    {
        if (user.IsDemoUser)
        {
            return UserConsts.DemoToken;
        }

        var token = CreateToken();
        _context.UserTokens.Add(new UserToken(user, token)
        {
            Expires = expires
        });

        await _context.SaveChangesAsync();
        return token;
    }

    /// <summary>
    /// Get the user's persistent access token.
    /// </summary>
    public async Task<string?> GetPersistentToken(User user) => (await _context.UserTokens
        .Where(ut => ut.Expires == DateTime.MaxValue)
        .Where(ut => ut.UserId == user.Id)
        .FirstOrDefaultAsync())?.Token;

    /// <summary>
    /// Get the user's current workout.
    /// </summary>
    public async Task<UserWorkout?> GetCurrentWorkout(User user, DateOnly? date = null, bool includeVariations = false)
    {
        date ??= user.TodayOffset;
        var query = _context.UserWorkouts.TagWithCallSite();

        if (includeVariations)
        {
            query = query.Include(uw => uw.UserWorkoutVariations);
        }

        return await query.Where(n => n.UserId == user.Id)
            .Where(n => n.Date <= date)
            // Checking for variations because we create a dummy newsletter
            // ... to advance the workout split and we want actual workouts.
            .Where(n => n.UserWorkoutVariations.Any())
            // Only select the most recent newsletter.
            .OrderByDescending(n => n.Date).ThenByDescending(n => n.Id)
            .IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get the last <paramref name="count"/> workouts for the user. Excludes the current workout.
    /// </summary>
    public async Task<IList<PastWorkout>> GetPastWorkouts(User user, int? count = null)
    {
        var query = _context.UserWorkouts.IgnoreQueryFilters().AsNoTracking()
            // Including all workouts each day b/c we allow multiple per day.
            // Check for workout variations because we create a dummy record
            // ... to advance the workout split, and we want actual workouts.
            .Where(uw => uw.UserWorkoutVariations.Any())
            // Do not show backfill workouts to users.
            .Where(uw => uw.Date >= user.CreatedDate)
            // Do not include future workouts in this.
            .Where(uw => uw.Date <= user.TodayOffset)
            .Where(uw => uw.UserId == user.Id)
            .OrderByDescending(uw => uw.Date)
            .ThenByDescending(uw => uw.Id);

        if (user.IsDemoUser)
        {
            // Select the most recent workout per day. Order after grouping.
            return await query.GroupBy(n => n.Date).OrderByDescending(n => n.Key)
                .Select(g => new PastWorkout(g.OrderByDescending(n => n.Id).First()))
                .Take(count ?? 7).IgnoreQueryFilters().AsNoTracking()
                .ToListAsync();
        }

        // Skip the current workout and return the next seven or so.
        return await query.Select(uw => new PastWorkout(uw)).Skip(1).Take(count ?? 7).ToListAsync();
    }

    /// <summary>
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    public async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.UserWorkouts
            .AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync(n => n.IsDeloadWeek);

        // Last deload started just this week. Keep the deload going until the week is over.
        if (lastDeload?.Date.StartOfWeek(UserConsts.StartOfDeloadWeek) == DateHelpers.Today.StartOfWeek(UserConsts.StartOfDeloadWeek))
        {
            return (needsDeload: true, timeUntilDeload: TimeSpan.Zero);
        }

        // Grab the number of days remaining until the start of the next deload week.
        // Using StartOfNextWeek b/c we want to start deloading after the week ends.
        var timeUntilDeload = TimeSpan.FromDays(lastDeload switch
        {
            // The user hasn't deloaded, find the next deload week using the created date.
            // Assumes the week that the user joined is a deload week—first week may be long.
            null => user.CreatedDate.StartOfNextWeek(UserConsts.StartOfDeloadWeek).DayNumber,
            // The user has had a deload week, find the next deload date using the deload date.
            not null => lastDeload.Date.StartOfNextWeek(UserConsts.StartOfDeloadWeek).DayNumber,
            // How far away the last deload needs to be before another deload.
        } - DateHelpers.Today.AddWeeks(-user.DeloadAfterXWeeks).DayNumber);

        return (timeUntilDeload <= TimeSpan.Zero, timeUntilDeload);
    }

    /// <summary>
    /// Get the user's current workout rotation.
    /// </summary>
    public async Task<(WorkoutRotationDto?, Frequency)> GetCurrentWorkoutRotation(User user, DateOnly? date = null)
    {
        date ??= user.TodayOffset;
        var currentWorkout = await GetCurrentWorkout(user, date, includeVariations: false);
        if (currentWorkout?.Date == date)
        {
            return (currentWorkout.Rotation.AsType<WorkoutRotationDto>()!, currentWorkout.Frequency);
        }

        return await GetNextRotation(user, user.ActualFrequency(date), date);
    }

    /// <summary>
    /// Calculates the user's next newsletter rotation from the previous newsletter.
    /// 
    /// May return a null rotation when the user has a rest day.
    /// </summary>
    public virtual async Task<(WorkoutRotationDto? rotation, Frequency frequency)> GetNextRotation(User user, Frequency frequency, DateOnly? date = null)
    {
        // Demo user should alternate each new day.
        return ((await GetUpcomingRotations(user, frequency, date, sameForToday: user.IsDemoUser)).FirstOrDefault(), frequency);
    }

    /// <summary>
    /// Grab the WorkoutRotation[] for the specified frequency.
    /// </summary>
    public async Task<WorkoutSplit> GetUpcomingRotations(User user, Frequency frequency, DateOnly? date = null, bool sameForToday = false)
    {
        date ??= user.TodayOffset;
        // Not checking for dummy workouts here, we want those to apply to alter the next workout rotation.
        var previousNewsletter = await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .Where(n => sameForToday ? (n.Date < date) : (n.Date <= date))
            .OrderByDescending(n => n.Date)
            // Select only the most recent.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        return new WorkoutSplit(frequency, user, previousNewsletter?.Rotation);
    }

    /// <summary>
    /// Grab the WorkoutRotation[] for the specified frequency.
    /// </summary>
    public async Task<IList<WorkoutRotationDto>> GetWeeklyRotations(User user, Frequency frequency)
    {
        // Not checking for dummy workouts here, we want those to apply to alter the next workout rotation.
        var previousNewsletter = await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .Where(n => n.Date < user.StartOfWeekOffset)
            .OrderByDescending(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        return new WorkoutSplit(frequency, user, previousNewsletter?.Rotation).Take(user.WorkoutsDays).ToList();
    }

    /// <summary>
    /// Get the user's weekly training volume for each muscle group.
    /// 
    /// Returns `null` when the user is new to fitness.
    /// </summary>
    /// <param name="rawValues">
    /// If true, returns how much left of a muscle group to work per week.
    /// If false, returns returns how much a muscle group has been worked.
    /// </param>
    public async Task<(double weeks, IDictionary<MusculoskeletalSystem, int?>? volume)> GetWeeklyMuscleVolume(User user, int weeks, bool rawValues = false, bool tul = false, bool includeToday = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weeks, 1);

        var (strengthWeeks, weeklyMuscleVolumeFromStrengthWorkouts) = await GetWeeklyMuscleVolumeFromStrengthWorkouts(user, weeks, includeToday: includeToday);
        var (_, weeklyMuscleVolumeFromMobilityWorkouts) = await GetWeeklyMuscleVolumeFromMobilityWorkouts(user, weeks, includeToday: includeToday);
        var userMuscleTargets = UserMuscleStrength.MuscleTargets.ToDictionary(mt => mt.Key, mt =>
        {
            var range = user.UserMuscleStrengths.FirstOrDefault(ums => ums.MuscleGroup == mt.Key)?.Range ?? mt.Value;
            var target = tul ? range.End.Value : range.Start.Value; // Buffer the volume with an extra week of data.
            return (int)Math.Ceiling(rawValues ? target + (target / (double)weeks) : target);
        });

        return (weeks: strengthWeeks, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m, m =>
        {
            if (weeklyMuscleVolumeFromStrengthWorkouts[m].HasValue && weeklyMuscleVolumeFromMobilityWorkouts[m].HasValue)
            {
                return rawValues ? userMuscleTargets[m] - (weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault())
                    : weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault();
            }

            // Not using the mobility value if the strength value doesn't exist b/c that results in uneven muscle target volumes.
            return rawValues ? userMuscleTargets[m] - weeklyMuscleVolumeFromStrengthWorkouts[m]
                : weeklyMuscleVolumeFromStrengthWorkouts[m];
        }));
    }

    private async Task<(double weeks, IDictionary<MusculoskeletalSystem, int?> volume)> GetWeeklyMuscleVolumeFromStrengthWorkouts(User user, int weeks, bool includeToday = false)
    {
        // Split queries for performance.
        var userWorkoutIds = await GetUserWorkoutIds(user, weeks, strengthening: true, includeToday: includeToday);
        var strengthNewsletters = await _context.UserWorkoutVariations.TagWithCallSite()
            .Where(n => userWorkoutIds.Contains(n.UserWorkoutId))
            // Only select variations that worked a strengthening intensity.
            .Where(nv => UserConsts.MuscleTargetSections.HasFlag(nv.Section))
            .Select(nv => new
            {
                nv.UserWorkout.Date,
                nv.Variation.Strengthens,
                nv.Variation.Stabilizes,
                nv.Variation.GetProficiency(nv.Section, nv.UserWorkout.Intensity)!.Volume,
                UserVariation = nv.Variation.UserVariations.FirstOrDefault(uv => uv.UserId == user.Id && uv.Section == nv.Section),
                UserVariationLog = nv.Variation.UserVariations.First(uv => uv.UserId == user.Id && uv.Section == nv.Section).UserVariationLogs.Where(uvl => uvl.Date <= nv.UserWorkout.Date).OrderByDescending(uvl => uvl.Date).FirstOrDefault(),
            }).IgnoreQueryFilters().AsNoTracking().ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (strengthNewsletters.Count != 0)
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data. Use the max newsletter date instead of today for backfilling support.
            var endDate = includeToday ? strengthNewsletters.Max(n => n.Date) : strengthNewsletters.Max(n => n.Date).EndOfWeek();
            var actualWeeks = (endDate.DayNumber - strengthNewsletters.Min(n => n.Date).StartOfWeek().DayNumber) / 7d;
            // User must have more than one week of data before we return anything.
            if (actualWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
            {
                var monthlyMuscles = strengthNewsletters.Select(nv =>
                {
                    var userIsNewWeight = GetUserIsNewWeight(user, nv.Date);
                    var isolationWeight = (nv.Strengthens | nv.Stabilizes).PopCount() <= UserConsts.IsolationStrengthensMax ? user.WeightIsolationXTimesMore : 1;
                    var volume = nv.UserVariationLog?.GetProficiency()?.Volume ?? nv.UserVariation?.GetProficiency()?.Volume ?? nv.Volume;

                    return new
                    {
                        nv.Strengthens,
                        nv.Stabilizes,
                        StrengthVolume = volume * userIsNewWeight * isolationWeight,
                        SecondaryVolume = volume * userIsNewWeight * isolationWeight / user.WeightSecondaryXTimesLess
                    };
                }).ToList();

                var coreMuscles = MuscleGroupExtensions.Core();
                return (weeks: actualWeeks, volume: UserMuscleStrength.MuscleTargets.Keys
                    .ToDictionary(m => m, m => (int?)Convert.ToInt64((
                            monthlyMuscles.Sum(mm => mm.Strengthens.HasFlag(m) ? (coreMuscles.Contains(m) ? mm.StrengthVolume / user.WeightCoreXTimesLess : mm.StrengthVolume) : 0)
                            + monthlyMuscles.Sum(mm => mm.Stabilizes.HasFlag(m) ? (coreMuscles.Contains(m) ? mm.SecondaryVolume / user.WeightCoreXTimesLess : mm.SecondaryVolume) : 0)
                        ) / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m, m => (int?)null));
    }

    private async Task<(double weeks, IDictionary<MusculoskeletalSystem, int?> volume)> GetWeeklyMuscleVolumeFromMobilityWorkouts(User user, int weeks, bool includeToday = false)
    {
        // Split queries for performance.
        var userWorkoutIds = await GetUserWorkoutIds(user, weeks, strengthening: false, includeToday: includeToday);
        var mobilityNewsletters = await _context.UserWorkoutVariations.TagWithCallSite()
            .Where(n => userWorkoutIds.Contains(n.UserWorkoutId))
            // Only select variations that worked a strengthening intensity.
            .Where(nv => UserConsts.MuscleTargetSections.HasFlag(nv.Section))
            .Select(nv => new
            {
                nv.UserWorkout.Date,
                nv.Variation.Strengthens,
                nv.Variation.Stabilizes,
                nv.Variation.GetProficiency(nv.Section, nv.UserWorkout.Intensity)!.Volume,
                UserVariation = nv.Variation.UserVariations.FirstOrDefault(uv => uv.UserId == user.Id && uv.Section == nv.Section),
                UserVariationLog = nv.Variation.UserVariations.First(uv => uv.UserId == user.Id && uv.Section == nv.Section).UserVariationLogs.Where(uvl => uvl.Date <= nv.UserWorkout.Date).OrderByDescending(uvl => uvl.Date).FirstOrDefault(),
            }).IgnoreQueryFilters().AsNoTracking().ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (mobilityNewsletters.Count != 0)
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data. Use the max newsletter date instead of today for backfilling support.
            var endDate = includeToday ? mobilityNewsletters.Max(n => n.Date) : mobilityNewsletters.Max(n => n.Date).EndOfWeek();
            var actualWeeks = (endDate.DayNumber - mobilityNewsletters.Min(n => n.Date).StartOfWeek().DayNumber) / 7d;
            // User must have more than one week of data before we return anything.
            if (actualWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
            {
                var monthlyMuscles = mobilityNewsletters.Select(nv =>
                {
                    var userIsNewWeight = GetUserIsNewWeight(user, nv.Date);
                    var isolationWeight = (nv.Strengthens | nv.Stabilizes).PopCount() <= UserConsts.IsolationStrengthensMax ? user.WeightIsolationXTimesMore : 1;
                    var volume = nv.UserVariationLog?.GetProficiency()?.Volume ?? nv.UserVariation?.GetProficiency()?.Volume ?? nv.Volume;

                    return new
                    {
                        nv.Strengthens,
                        nv.Stabilizes,
                        StrengthVolume = volume * userIsNewWeight * isolationWeight,
                        SecondaryVolume = volume * userIsNewWeight * isolationWeight / user.WeightSecondaryXTimesLess
                    };
                }).ToList();

                var coreMuscles = MuscleGroupExtensions.Core();
                return (weeks: actualWeeks, volume: UserMuscleStrength.MuscleTargets.Keys
                    .ToDictionary(m => m, m => (int?)Convert.ToInt64((
                            monthlyMuscles.Sum(mm => mm.Strengthens.HasFlag(m) ? (coreMuscles.Contains(m) ? mm.StrengthVolume / user.WeightCoreXTimesLess : mm.StrengthVolume) : 0)
                            + monthlyMuscles.Sum(mm => mm.Stabilizes.HasFlag(m) ? (coreMuscles.Contains(m) ? mm.SecondaryVolume / user.WeightCoreXTimesLess : mm.SecondaryVolume) : 0)
                        ) / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m, m => (int?)null));
    }

    /// <param name="includeToday">
    /// If true, returns workouts up-to and including today.
    /// If false, returns workouts up-to the start of the current week.
    /// </param>
    private async Task<IList<int>> GetUserWorkoutIds(User user, int weeks, bool strengthening, bool includeToday = false)
    {
        // NOTE: When backfilling, the start of week is incorrect causing the weekly targets change throughout the week.
        // Not using the user's offset date because the user can alter that
        // ... and we don't want this to change in the middle of the week.
        var startOfWeek = DateHelpers.Today.StartOfWeek();
        var query = _context.UserWorkouts.TagWithCallSite()
            .IgnoreQueryFilters().AsNoTracking()
            .Where(n => n.UserId == user.Id)
            // Checking for variations b/c we create a dummy newsletter to advance the workout split.
            .Where(n => n.UserWorkoutVariations.Any())
            // Look at strengthening/mobility workouts only.
            .Where(n => strengthening ? n.Frequency != Frequency.Mobility : n.Frequency == Frequency.Mobility)
            // Choose newsletters earlier than the start of the week X weeks ago.
            .Where(n => n.Date >= startOfWeek.AddDays(-7 * weeks))
            // Include the current week or exclude this week.
            .Where(n => includeToday || n.Date < startOfWeek);

        if (user.Features.HasAnyFlag(Features.Demo | Features.Test))
        {
            // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
            return await query.GroupBy(n => n.Date).Select(g => g.OrderByDescending(n => n.Id).First().Id).ToListAsync();
        }

        // Otherwise, we want to select all workouts being sent because we allow up to two mobility workouts per day.
        return await query.Select(g => g.Id).ToListAsync();
    }

    /// <summary>
    /// Gradually lessen the weight when switching away from IsNewToFitness.
    /// </summary>
    /// <param name="date">The date of the workout that we're scaling.</param>
    private static double GetUserIsNewWeight(User user, DateOnly date)
    {
        // Gradually ramp the user weight. Using the workout date so the weight only changes for new workouts.
        var daysSinceSeasoned = user.SeasonedDate.HasValue ? date.DayNumber - user.SeasonedDate.Value.DayNumber : 0;
        return Math.Clamp(UserConsts.WeightUserIsNewXTimesMore - (daysSinceSeasoned * .01), 1, UserConsts.WeightUserIsNewXTimesMore);
    }
}

