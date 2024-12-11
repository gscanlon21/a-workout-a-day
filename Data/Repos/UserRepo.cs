using Core.Code.Exceptions;
using Core.Dtos.Newsletter;
using Core.Models.Exercise;
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
    public async Task<UserWorkout?> GetCurrentWorkout(User user, bool includeVariations = false)
    {
        var query = _context.UserWorkouts.TagWithCallSite();

        if (includeVariations)
        {
            query = query.Include(uw => uw.UserWorkoutVariations);
        }

        return await query.Where(n => n.UserId == user.Id)
            .Where(n => n.Date <= user.TodayOffset)
            // Checking for variations because we create a dummy newsletter
            // ... to advance the workout split and we want actual workouts.
            .Where(n => n.UserWorkoutVariations.Any())
            // Only select the most recent newsletter.
            .OrderByDescending(n => n.Date).ThenByDescending(n => n.Id)
            .IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get the last 7 days of workouts for the user. Excludes the current workout.
    /// </summary>
    public async Task<IList<PastWorkout>> GetPastWorkouts(User user, int? count = null)
    {
        return await _context.UserWorkouts.Where(uw => uw.UserId == user.Id)
            // Check for workout variations because we create a dummy record
            // ... to advance the workout split, and we want actual workouts.
            .Where(n => n.UserWorkoutVariations.Any())
            .Where(n => n.Date < user.TodayOffset)
            // Select the most recent workout per day. Order after grouping.
            .GroupBy(n => n.Date).OrderByDescending(n => n.Key)
            .Select(g => new PastWorkout(g.OrderByDescending(n => n.Id).First()))
            .Take(count ?? 7).IgnoreQueryFilters().AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    public async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync(n => n.IsDeloadWeek);

        // Grabs the date of Sunday of the current week.
        var currentWeekStart = DateHelpers.Today.AddDays(-1 * (int)DateHelpers.Today.DayOfWeek);
        // Grabs the Sunday that was the start of the last deload.
        var lastDeloadStartOfWeek = lastDeload != null ? lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) : DateOnly.MinValue;
        // Grabs the Sunday at or before the user's created date.
        var createdDateStartOfWeek = user.CreatedDate.AddDays(-1 * (int)user.CreatedDate.DayOfWeek);
        // How far away the last deload need to be before another deload.
        var countUpToNextDeload = DateHelpers.Today.AddDays(-7 * user.DeloadAfterXWeeks);

        bool isSameWeekAsLastDeload = lastDeload != null && lastDeloadStartOfWeek == currentWeekStart;
        TimeSpan timeUntilDeload = (isSameWeekAsLastDeload, lastDeload) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(createdDateStartOfWeek.DayNumber - countUpToNextDeload.DayNumber),
            // Calculate the next deload date using the last deload date.
            (false, not null) => TimeSpan.FromDays(lastDeloadStartOfWeek.DayNumber - countUpToNextDeload.DayNumber),
            // Dates are the same week. Keep the deload going until the week is over.
            _ => TimeSpan.Zero
        };

        return (timeUntilDeload <= TimeSpan.Zero, timeUntilDeload);
    }

    /// <summary>
    /// Get the user's current workout rotation.
    /// </summary>
    public async Task<(WorkoutRotationDto?, Frequency)> GetCurrentWorkoutRotation(User user)
    {
        var currentWorkout = await GetCurrentWorkout(user, includeVariations: false);
        var upcomingRotations = await GetUpcomingRotations(user, user.ActualFrequency);
        if (currentWorkout?.Date == user.TodayOffset)
        {
            return (currentWorkout.Rotation.AsType<WorkoutRotationDto>()!, currentWorkout.Frequency);
        }
        else
        {
            return (upcomingRotations.FirstOrDefault(), user.ActualFrequency);
        }
    }

    /// <summary>
    /// Calculates the user's next newsletter rotation from the previous newsletter.
    /// 
    /// May return a null rotation when the user has a rest day.
    /// </summary>
    public virtual async Task<(Frequency frequency, WorkoutRotationDto? rotation)> GetNextRotation(User user)
    {
        // Demo user should alternate each new day.
        var rotation = (await GetUpcomingRotations(user, user.ActualFrequency, sameForToday: user.IsDemoUser)).FirstOrDefault();
        return (user.ActualFrequency, rotation);
    }

    /// <summary>
    /// Grab the WorkoutRotation[] for the specified frequency.
    /// </summary>
    public async Task<WorkoutSplit> GetUpcomingRotations(User user, Frequency frequency, bool sameForToday = false)
    {
        // Not checking for dummy workouts here, we want those to apply to alter the next workout rotation.
        var previousNewsletter = await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .Where(n => sameForToday ? (n.Date < user.TodayOffset) : (n.Date <= user.TodayOffset))
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
            return tul ? range.End.Value : range.Start.Value + UserConsts.IncrementMuscleTargetBy;
        });

        return (weeks: strengthWeeks, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m,
            m =>
            {
                if (weeklyMuscleVolumeFromStrengthWorkouts[m].HasValue && weeklyMuscleVolumeFromMobilityWorkouts[m].HasValue)
                {
                    return rawValues ? userMuscleTargets[m] - (weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault())
                        : weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault();
                }

                // Not using the mobility value if the strength value doesn't exist because
                // ... we don't want muscle target adjustments to apply to strength workouts using mobility muscle volumes.
                return rawValues ? userMuscleTargets[m] - weeklyMuscleVolumeFromStrengthWorkouts[m]
                    : weeklyMuscleVolumeFromStrengthWorkouts[m];
            })
        );
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
                nv.Variation.GetProficiency(nv.Section, nv.UserWorkout.Intensity).Volume,
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

                return (weeks: actualWeeks, volume: UserMuscleStrength.MuscleTargets.Keys
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32((
                            monthlyMuscles.Sum(mm => mm.Strengthens.HasFlag(m) ? mm.StrengthVolume : 0)
                            + monthlyMuscles.Sum(mm => mm.Stabilizes.HasFlag(m) ? mm.SecondaryVolume : 0)
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
                nv.Variation.GetProficiency(nv.Section, nv.UserWorkout.Intensity).Volume,
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

                return (weeks: actualWeeks, volume: UserMuscleStrength.MuscleTargets.Keys
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32((
                            monthlyMuscles.Sum(mm => mm.Strengthens.HasFlag(m) ? mm.StrengthVolume : 0)
                            + monthlyMuscles.Sum(mm => mm.Stabilizes.HasFlag(m) ? mm.SecondaryVolume : 0)
                        ) / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m, m => (int?)null));
    }

    private async Task<IList<int>> GetUserWorkoutIds(User user, int weeks, bool strengthening, bool includeToday = false)
    {
        // Not using the user's offset date because the user can alter that.
        var startOfWeek = DateHelpers.Today.StartOfWeek();
        return await _context.UserWorkouts.TagWithCallSite()
            .IgnoreQueryFilters().AsNoTracking()
            .Where(n => n.UserId == user.Id)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.UserWorkoutVariations.Any())
            // Look at strengthening/mobility workouts only that are within the last X weeks.
            .Where(n => strengthening ? n.Frequency != Frequency.OffDayStretches : n.Frequency == Frequency.OffDayStretches)
            // Choose newsletters earlier than the start of the week 7 weeks ago.
            .Where(n => n.Date >= startOfWeek.AddDays(-7 * weeks))
            // Include the current week or exclude this week.
            .Where(n => includeToday || n.Date < startOfWeek)
            // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
            .GroupBy(n => n.Date).Select(g => g.OrderByDescending(n => n.Id).First().Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gradually lessen the weight when switching away from IsNewToFitness.
    /// </summary>
    /// <param name="date">The date of the workout that we're scaling.</param>
    private static double GetUserIsNewWeight(User user, DateOnly date)
    {
        const double weightUserIsNewXTimesMore = 1.25;

        // Gradually ramp the user down to the normal scale.
        var daysSinceSeasoned = user.SeasonedDate.HasValue ? date.DayNumber - user.SeasonedDate.Value.DayNumber : 0;
        return Math.Clamp(weightUserIsNewXTimesMore - (daysSinceSeasoned * .01), 1, weightUserIsNewXTimesMore);
    }
}

