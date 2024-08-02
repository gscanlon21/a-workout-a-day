using Core.Code.Exceptions;
using Core.Consts;
using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Web.Code;

namespace Data.Repos;

/// <summary>
/// User helpers.
/// </summary>
public class UserRepo
{
    // Keep this relatively low so it is less jarring when the user switches away from IsNewToFitness.
    private const double WeightUserIsNewXTimesMore = 1.25;

    private readonly CoreContext _context;

    public UserRepo(CoreContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Grab a user from the db with a specific token.
    /// </summary>
    public async Task<User> GetUserStrict(string? email, string? token,
        bool includeUserExerciseVariations = false,
        bool includeExerciseVariations = false,
        bool includeMuscles = false,
        bool includeFrequencies = false,
        bool allowDemoUser = false)
    {
        return await GetUser(email, token,
            includeUserExerciseVariations: includeUserExerciseVariations,
            includeExerciseVariations: includeExerciseVariations,
            includeMuscles: includeMuscles,
            includeFrequencies: includeFrequencies,
            allowDemoUser: allowDemoUser) ?? throw new UserException("User is null.");
    }

    /// <summary>
    /// Grab a user from the db with a specific token.
    /// </summary>
    public async Task<User?> GetUser(string? email, string? token,
        bool includeUserExerciseVariations = false,
        bool includeExerciseVariations = false,
        bool includeMuscles = false,
        bool includeFrequencies = false,
        bool allowDemoUser = false)
    {
        if (email == null || token == null)
        {
            return null;
        }

        IQueryable<User> query = _context.Users.AsSplitQuery().TagWithCallSite();

        if (includeMuscles)
        {
            query = query.Include(u => u.UserMuscleStrengths);
            query = query.Include(u => u.UserMuscleMobilities);
            query = query.Include(u => u.UserMuscleFlexibilities);
        }

        if (includeFrequencies)
        {
            query = query.Include(u => u.UserFrequencies);
        }

        if (includeExerciseVariations)
        {
            query = query.Include(u => u.UserExercises).ThenInclude(ue => ue.Exercise)
                         .Include(u => u.UserVariations).ThenInclude(uv => uv.Variation);
        }
        else if (includeUserExerciseVariations)
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
    /// Get the user's current workout.
    /// </summary>
    public async Task<UserWorkout?> GetCurrentWorkout(User user)
    {
        return await _context.UserWorkouts.AsNoTracking().TagWithCallSite()
            .Include(uw => uw.UserWorkoutVariations)
            .Where(n => n.UserId == user.Id)
            .Where(n => n.Date <= user.TodayOffset)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split and we want actual workouts.
            .Where(n => n.UserWorkoutVariations.Any())
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .OrderByDescending(n => n.Date)
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get the last 7 days of workouts for the user. Excludes the current workout.
    /// </summary>
    public async Task<IList<UserWorkout>> GetPastWorkouts(User user)
    {
        return (await _context.UserWorkouts
            .Where(uw => uw.UserId == user.Id)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split and we want actual workouts.
            .Where(n => n.UserWorkoutVariations.Any())
            .Where(n => n.Date < user.TodayOffset)
            // Only select 1 workout per day, the most recent.
            .GroupBy(n => n.Date)
            .Select(g => new
            {
                g.Key,
                // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
                // Dummy records that are created when the user advances their workout split may also have the same date.
                Workout = g.OrderByDescending(n => n.Id).First()
            })
            .OrderByDescending(n => n.Key)
            .Take(7)
            .ToListAsync())
            .Select(n => n.Workout)
            .ToList();
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
        var currentWorkout = await GetCurrentWorkout(user);
        var upcomingRotations = await GetUpcomingRotations(user, user.ActualFrequency);
        if (currentWorkout?.Date == user.TodayOffset)
        {
            return (currentWorkout.Rotation.AsType<WorkoutRotationDto, WorkoutRotation>()!, currentWorkout.Frequency);
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
    public async Task<(Frequency frequency, WorkoutRotationDto? rotation)> GetNextRotation(User user)
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
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        return new WorkoutSplit(frequency, user.AsType<UserDto, User>()!, previousNewsletter?.Rotation.AsType<WorkoutRotationDto, WorkoutRotation>()!);
    }

    /// <summary>
    /// Get the user's weekly training volume for each muscle group.
    /// 
    /// Returns `null` when the user is new to fitness.
    /// </summary>
    public async Task<(double weeks, IDictionary<MuscleGroups, int?>? volume)> GetWeeklyMuscleVolume(User user, int weeks, bool includeToday = false)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weeks, 1);

        var (strengthWeeks, weeklyMuscleVolumeFromStrengthWorkouts) = await GetWeeklyMuscleVolumeFromStrengthWorkouts(user, weeks, includeToday: includeToday);
        var (_, weeklyMuscleVolumeFromMobilityWorkouts) = await GetWeeklyMuscleVolumeFromMobilityWorkouts(user, weeks, includeToday: includeToday);

        return (weeks: strengthWeeks, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m,
            m =>
            {
                if (weeklyMuscleVolumeFromStrengthWorkouts[m].HasValue && weeklyMuscleVolumeFromMobilityWorkouts[m].HasValue)
                {
                    return weeklyMuscleVolumeFromStrengthWorkouts[m].GetValueOrDefault() + weeklyMuscleVolumeFromMobilityWorkouts[m].GetValueOrDefault();
                }

                // Not using the mobility value if the strength value doesn't exist because
                // ... we don't want muscle target adjustments to apply to strength workouts using mobility muscle volumes.
                return weeklyMuscleVolumeFromStrengthWorkouts[m];
            })
        );
    }

    private async Task<(double weeks, IDictionary<MuscleGroups, int?> volume)> GetWeeklyMuscleVolumeFromStrengthWorkouts(User user, int weeks, bool includeToday = false)
    {
        var strengthNewsletterGroups = await _context.UserWorkouts
            .AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Only look at records where the user is not new to fitness.
            .Where(n => user.IsNewToFitness || n.Date > user.SeasonedDate)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.UserWorkoutVariations.Any())
            // Look at strengthening workouts only that are within the last X weeks.
            .Where(n => n.Frequency != Frequency.OffDayStretches)
            .Where(n => n.Date >= user.StartOfWeekOffset.AddDays(-7 * weeks))
            .Where(n => includeToday || n.Date < user.StartOfWeekOffset)
            .GroupBy(n => n.Date)
            .Select(g => new
            {
                g.Key,
                // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
                NewsletterVariations = g.OrderByDescending(n => n.Id).First().UserWorkoutVariations
                    // Only select variations that worked a strengthening intensity.
                    .Where(nv => UserConsts.MuscleTargetSections.HasFlag(nv.Section))
                    .Select(nv => new
                    {
                        nv.Variation.StrengthMuscles,
                        nv.Variation.SecondaryMuscles,
                        nv.Variation.GetProficiency(nv.Section, g.OrderByDescending(n => n.Id).First().Intensity).Volume,
                        UserVariation = nv.Variation.UserVariations.FirstOrDefault(uv => uv.UserId == user.Id && uv.Section == nv.Section),
                        UserVariationLog = nv.Variation.UserVariations.First(uv => uv.UserId == user.Id && uv.Section == nv.Section).UserVariationLogs.Where(uvl => uvl.Date <= g.Key).OrderByDescending(uvl => uvl.Date).FirstOrDefault(),
                    })
            }).ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (strengthNewsletterGroups.Count != 0)
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data.
            var endDate = includeToday ? user.TodayOffset : user.StartOfWeekOffset;
            var actualWeeks = (endDate.DayNumber - strengthNewsletterGroups.Min(n => n.Key).StartOfWeek().DayNumber) / 7d;
            // User must have more than one week of data before we return anything.
            if (actualWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
            {
                var monthlyMuscles = strengthNewsletterGroups.SelectMany(ng => ng.NewsletterVariations.Select(nv =>
                {
                    var userIsNewWeight = user.IsNewToFitness ? WeightUserIsNewXTimesMore : 1;
                    var isolationWeight = (nv.StrengthMuscles | nv.SecondaryMuscles).PopCount() <= UserConsts.IsolationIsXStrengthMuscles ? user.WeightIsolationXTimesMore : 1;
                    var volume = nv.UserVariationLog?.GetProficiency()?.Volume ?? nv.UserVariation?.GetProficiency()?.Volume ?? nv.Volume;

                    return new
                    {
                        nv.StrengthMuscles,
                        nv.SecondaryMuscles,
                        StrengthVolume = volume * userIsNewWeight * isolationWeight,
                        SecondaryVolume = volume * userIsNewWeight * isolationWeight / user.WeightSecondaryMusclesXTimesLess
                    };
                })).ToList();

                return (weeks: actualWeeks, volume: UserMuscleStrength.MuscleTargets.Keys
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32((
                            monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.StrengthVolume : 0)
                            + monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.SecondaryVolume : 0)
                        ) / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m, m => (int?)null));
    }

    private async Task<(double weeks, IDictionary<MuscleGroups, int?> volume)> GetWeeklyMuscleVolumeFromMobilityWorkouts(User user, int weeks, bool includeToday = false)
    {
        var mobilityNewsletterGroups = await _context.UserWorkouts
            .AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Only look at records where the user is not new to fitness.
            .Where(n => user.IsNewToFitness || n.Date > user.SeasonedDate)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.UserWorkoutVariations.Any())
            // Look at mobility workouts only that are within the last X weeks.
            .Where(n => n.Frequency == Frequency.OffDayStretches)
            .Where(n => n.Date >= user.StartOfWeekOffset.AddDays(-7 * weeks))
            .Where(n => includeToday || n.Date < user.StartOfWeekOffset)
            .GroupBy(n => n.Date)
            .Select(g => new
            {
                g.Key,
                // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created and select first.
                NewsletterVariations = g.OrderByDescending(n => n.Id).First().UserWorkoutVariations
                    // Only select variations that worked a strengthening intensity.
                    .Where(nv => UserConsts.MuscleTargetSections.HasFlag(nv.Section))
                    .Select(nv => new
                    {
                        nv.Variation.StrengthMuscles,
                        nv.Variation.SecondaryMuscles,
                        nv.Variation.GetProficiency(nv.Section, g.OrderByDescending(n => n.Id).First().Intensity).Volume,
                        UserVariation = nv.Variation.UserVariations.FirstOrDefault(uv => uv.UserId == user.Id && uv.Section == nv.Section),
                        UserVariationLog = nv.Variation.UserVariations.First(uv => uv.UserId == user.Id && uv.Section == nv.Section).UserVariationLogs.Where(uvl => uvl.Date <= g.Key).OrderByDescending(uvl => uvl.Date).FirstOrDefault(),
                    })
            }).ToListAsync();

        // .Max/.Min throw exceptions when the collection is empty.
        if (mobilityNewsletterGroups.Count != 0)
        {
            // sa. Drop 4 weeks down to 3.5 weeks if we only have 3.5 weeks of data.
            var endDate = includeToday ? user.TodayOffset : user.StartOfWeekOffset;
            var actualWeeks = (endDate.DayNumber - mobilityNewsletterGroups.Min(n => n.Key).StartOfWeek().DayNumber) / 7d;
            // User must have more than one week of data before we return anything.
            if (actualWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
            {
                var monthlyMuscles = mobilityNewsletterGroups.SelectMany(ng => ng.NewsletterVariations.Select(nv =>
                {
                    var userIsNewWeight = user.IsNewToFitness ? WeightUserIsNewXTimesMore : 1;
                    var isolationWeight = (nv.StrengthMuscles | nv.SecondaryMuscles).PopCount() <= UserConsts.IsolationIsXStrengthMuscles ? user.WeightIsolationXTimesMore : 1;
                    var volume = nv.UserVariationLog?.GetProficiency()?.Volume ?? nv.UserVariation?.GetProficiency()?.Volume ?? nv.Volume;

                    return new
                    {
                        nv.StrengthMuscles,
                        nv.SecondaryMuscles,
                        StrengthVolume = volume * userIsNewWeight * isolationWeight,
                        SecondaryVolume = volume * userIsNewWeight * isolationWeight / user.WeightSecondaryMusclesXTimesLess
                    };
                })).ToList();

                return (weeks: actualWeeks, volume: UserMuscleStrength.MuscleTargets.Keys
                    .ToDictionary(m => m, m => (int?)Convert.ToInt32((
                            monthlyMuscles.Sum(mm => mm.StrengthMuscles.HasFlag(m) ? mm.StrengthVolume : 0)
                            + monthlyMuscles.Sum(mm => mm.SecondaryMuscles.HasFlag(m) ? mm.SecondaryVolume : 0)
                        ) / actualWeeks)
                    )
                );
            }
        }

        return (weeks: 0, volume: UserMuscleStrength.MuscleTargets.Keys.ToDictionary(m => m, m => (int?)null));
    }
}

