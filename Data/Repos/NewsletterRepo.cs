using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Data;
using Data.Data.Query;
using Data.Dtos.Newsletter;
using Data.Dtos.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.Repos;

public partial class NewsletterRepo
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// This week's Sunday date in UTC.
    /// </summary>
    protected static DateOnly StartOfWeek => Today.AddDays(-1 * (int)Today.DayOfWeek);

    private readonly CoreContext _context;
    private readonly UserRepo _userRepo;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NewsletterRepo> _logger;

    public NewsletterRepo(ILogger<NewsletterRepo> logger, CoreContext context, UserRepo userRepo, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _userRepo = userRepo;
        _context = context;
    }

    public async Task<IList<Entities.Footnote.Footnote>> GetFootnotes(int count = 1, FootnoteType ofType = FootnoteType.Bottom)
    {
        var footnotes = await _context.Footnotes
            // Has any flag
            .Where(f => (f.Type & ofType) != 0)
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return footnotes;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<NewsletterDto?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null)
    {
        var user = await _userRepo.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null)
        {
            return null;
        }

        _logger.Log(LogLevel.Information, "Building newsletter for user {Id}", user.Id);

        // User is a debug user. They should see the DebugNewsletter instead.
        if (user.Features.HasFlag(Features.Debug))
        {
            _logger.Log(LogLevel.Information, "Returning debug newsletter for user {Id}", user.Id);
            return await Debug(email, token);
        }

        if (date.HasValue && !user.Features.HasFlag(Features.Demo) && !user.Features.HasFlag(Features.Test))
        {
            var oldNewsletter = await _context.UserWorkouts.AsNoTracking()
                .Include(n => n.UserWorkoutExerciseVariations)
                .Where(n => n.User.Id == user.Id)
                // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
                .Where(n => n.UserWorkoutExerciseVariations.Any())
                .Where(n => n.Date == date)
                // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created.
                .OrderByDescending(n => n.Id)
                .FirstOrDefaultAsync();

            // A newsletter was found
            if (oldNewsletter != null)
            {
                _logger.Log(LogLevel.Information, "Returning old newsletter for user {Id}", user.Id);
                return await NewsletterOld(user, token, date.Value, oldNewsletter);
            }
            // A newsletter was not found and the date is not one we want to render a new newsletter for
            else if (date != Today)
            {
                _logger.Log(LogLevel.Information, "Returning no newsletter for user {Id}", user.Id);
                return null;
            }
            // Else continue on to render a new newsletter for today
        }

        if (user.RestDays.HasFlag(DaysExtensions.FromDate(Today)))
        {
            if (user.IncludeMobilityWorkouts)
            {
                _logger.Log(LogLevel.Information, "Returning off day newsletter for user {Id}", user.Id);
                return await OffDayNewsletter(user, token);
            }

            _logger.Log(LogLevel.Information, "Returning no newsletter for user {Id}", user.Id);
            return null;
        }

        _logger.Log(LogLevel.Information, "Returning on day newsletter for user {Id}", user.Id);
        return await OnDayNewsletter(user, token);
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    public async Task<NewsletterDto?> Debug(string email, string token)
    {
        // The debug user is disabled, not checking that or rest days.
        var user = await _userRepo.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true);
        if (user == null || user.RestDays.HasFlag(DaysExtensions.FromDate(Today))
            // User is not a debug user. They should see the Newsletter instead.
            || !user.Features.HasFlag(Features.Debug))
        {
            return null;
        }

        user.Verbosity = Verbosity.Debug;
        await AddMissingUserExerciseVariationRecords(user);
        var context = await BuildWorkoutContext(user, isMobilityWorkout: false);

        var debugExercises = await GetDebugExercises(user, count: 1);
        var newsletter = await CreateAndAddNewsletterToContext(context, exercises: debugExercises);
        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(user, token);
        var viewModel = new NewsletterDto(userViewModel, newsletter)
        {
            Equipment = equipmentViewModel,
            MainExercises = debugExercises,
            PrehabExercises = new List<ExerciseDto>(),
            RehabExercises = new List<ExerciseDto>(),
            SportsExercises = new List<ExerciseDto>(),
            WarmupExercises = new List<ExerciseDto>(),
            CooldownExercises = new List<ExerciseDto>(),
        };

        await UpdateLastSeenDate(debugExercises);

        return viewModel;
    }

    private async Task<WorkoutContext> BuildWorkoutContext(Entities.User.User user, bool isMobilityWorkout)
    {
        var frequency = isMobilityWorkout ? Frequency.OffDayStretches : user.Frequency;
        var todaysWorkoutRotation = await _userRepo.GetTodaysWorkoutRotation(user, frequency);
        (var needsDeload, var timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);
        // Add 1 because deloads occur after every x weeks, not on.
        var weeklyMuscles = await _userRepo.GetWeeklyMuscleVolume(user, weeks: Math.Max(UserConsts.DeloadAfterEveryXWeeksDefault, user.DeloadAfterEveryXWeeks + 1));
        var userAllWorkedMuscles = (await _userRepo.GetCurrentAndUpcomingRotations(user)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups);

        return new WorkoutContext()
        {
            User = user,
            Frequency = frequency,
            NeedsDeload = needsDeload,
            TimeUntilDeload = timeUntilDeload,
            UserAllWorkedMuscles = userAllWorkedMuscles,
            WorkoutRotation = todaysWorkoutRotation,
            WeeklyMuscles = weeklyMuscles,
        };
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<NewsletterDto?> OnDayNewsletter(Entities.User.User user, string token)
    {
        await AddMissingUserExerciseVariationRecords(user);

        var context = await BuildWorkoutContext(user, isMobilityWorkout: false);

        // Choose cooldown first, these are the easiest so we want to work variations that can be a part of two or more sections here.
        var cooldownExercises = await GetCooldownExercises(context);
        var warmupExercises = await GetWarmupExercises(context,
            // Never work the same variation twice
            excludeVariations: cooldownExercises);

        var coreExercises = await GetCoreExercises(context,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises));

        var functionalExercises = await GetFunctionalExercises(context,
            // Never work the same variation twice
            excludeVariations: cooldownExercises.Concat(warmupExercises).Concat(coreExercises));

        var sportsExercises = await GetSportsExercises(context,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            excludeGroups: functionalExercises.Concat(coreExercises),
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            excludeExercises: functionalExercises.Concat(coreExercises),
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises).Concat(functionalExercises),
            // Unset muscles that have already been worked by the core, functional and sports exercises.
            workedMusclesDict: coreExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                    // Weight secondary muscles as half.
                    addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2)
                )
            ));

        // Lower the intensity to reduce the risk of injury from heavy-weighted isolation exercises.
        var accessoryExercises = await GetAccessoryExercises(context,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            excludeGroups: functionalExercises.Concat(sportsExercises).Concat(coreExercises),
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            excludeExercises: functionalExercises.Concat(sportsExercises).Concat(coreExercises),
            // Never work the same variation twice
            excludeVariations: functionalExercises.Concat(sportsExercises).Concat(warmupExercises).Concat(cooldownExercises).Concat(coreExercises),
            // Unset muscles that have already been worked by the core, functional and sports exercises.
            workedMusclesDict: coreExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                    addition: sportsExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                        // Weight secondary muscles as half.
                        addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2,
                            addition: sportsExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2)
                        )
                    )
                )
            ));

        var rehabExercises = await GetRehabExercises(context);
        // Grab strengthening prehab exercises.
        var prehabExercises = await GetPrehabExercises(context, strengthening: true,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises).Concat(functionalExercises).Concat(accessoryExercises).Concat(sportsExercises));

        var newsletter = await CreateAndAddNewsletterToContext(context,
            exercises: rehabExercises.Concat(warmupExercises).Concat(sportsExercises).Concat(functionalExercises.Concat(accessoryExercises).Concat(coreExercises)).Concat(prehabExercises).Concat(cooldownExercises).ToList()
        );

        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(user, token)
        {
            TimeUntilDeload = context.TimeUntilDeload,
        };
        var viewModel = new NewsletterDto(userViewModel, newsletter)
        {
            Equipment = equipmentViewModel,
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            MainExercises = functionalExercises.Concat(accessoryExercises).Concat(coreExercises).ToList(),
            SportsExercises = sportsExercises,
            CooldownExercises = cooldownExercises,
        };

        // Functional exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: functionalExercises,
            refreshAfter: StartOfWeek.AddDays(7 * user.RefreshFunctionalEveryXWeeks));
        // Accessory exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: accessoryExercises,
            refreshAfter: StartOfWeek.AddDays(7 * user.RefreshAccessoryEveryXWeeks));
        // Other exercises. Refresh every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(prehabExercises).Concat(rehabExercises).Concat(sportsExercises));

        return viewModel;
    }

    /// <summary>
    /// The mobility/stretch newsletter for days off strength training.
    /// </summary>
    private async Task<NewsletterDto?> OffDayNewsletter(Entities.User.User user, string token)
    {
        await AddMissingUserExerciseVariationRecords(user);

        var context = await BuildWorkoutContext(user, isMobilityWorkout: true);

        // Choose cooldown first, these are the easiest so we want to work variations that can be a part of two or more sections here.
        var cooldownExercises = await GetCooldownExercises(context);
        var warmupExercises = await GetWarmupExercises(context,
            // Never work the same variation twice
            excludeVariations: cooldownExercises);

        var coreExercises = await GetCoreExercises(context,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises));

        var rehabExercises = await GetRehabExercises(context);
        // Grab stretching prehab exercises
        var prehabExercises = await GetPrehabExercises(context, strengthening: false,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises));

        var newsletter = await CreateAndAddNewsletterToContext(context,
            exercises: rehabExercises.Concat(warmupExercises).Concat(coreExercises).Concat(prehabExercises).Concat(cooldownExercises).ToList()
        );

        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(user, token)
        {
            TimeUntilDeload = context.TimeUntilDeload,
        };
        var viewModel = new NewsletterDto(userViewModel, newsletter)
        {
            Equipment = equipmentViewModel,
            MainExercises = coreExercises,
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            CooldownExercises = cooldownExercises,
            SportsExercises = new List<ExerciseDto>()
        };

        // Refresh these exercises every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(prehabExercises).Concat(rehabExercises));

        return viewModel;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter based on a date.
    /// </summary>
    private async Task<NewsletterDto?> NewsletterOld(Entities.User.User user, string token, DateOnly date, Entities.Newsletter.UserWorkout newsletter)
    {
        await AddMissingUserExerciseVariationRecords(user);

        var prehabExercises = (await new QueryBuilder(Section.Prehab)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .WithExercises(options =>
            {
                options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, user.Verbosity, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault()))
            .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
            .ToList();

        var rehabExercises = (await new QueryBuilder(Section.Rehab)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .WithExercises(options =>
            {
                options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, user.Verbosity, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault()))
            .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
            .ToList();

        var warmupExercises = (await new QueryBuilder(Section.Warmup)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .WithExercises(options =>
            {
                options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, user.Verbosity, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault()))
            .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
            .ToList();

        var mainExercises = (await new QueryBuilder(Section.Main)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .WithExercises(options =>
            {
                options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Main, user.Verbosity, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault()))
            .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
            .ToList();

        var cooldownExercises = (await new QueryBuilder(Section.Cooldown)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .WithExercises(options =>
            {
                options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Cooldown, user.Verbosity, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault()))
            .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
            .ToList();

        var sportsExercises = (await new QueryBuilder(Section.Sports)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .WithExercises(options =>
            {
                options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Other, user.Verbosity, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault()))
            .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
            .ToList();

        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(user, token);
        return new NewsletterDto(userViewModel, newsletter)
        {
            Today = date,
            Equipment = equipmentViewModel,
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            MainExercises = mainExercises,
            SportsExercises = sportsExercises,
            CooldownExercises = cooldownExercises
        };
    }

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseDto>> GetDebugExercises(Entities.User.User user, int count = 1)
    {
        var baseQuery = _context.ExerciseVariations
            .Include(v => v.Exercise)
                .ThenInclude(e => e.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteExercise)
            .Include(ev => ev.Variation)
                .ThenInclude(i => i.Intensities)
            .Include(ev => ev.Variation)
                .ThenInclude(i => i.DefaultInstruction)
            .Include(v => v.Variation)
                .ThenInclude(i => i.Instructions.Where(eg => eg.Parent == null))
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Include(v => v.Variation)
                .ThenInclude(i => i.Instructions.Where(eg => eg.Parent == null))
                    .ThenInclude(eg => eg.Children)
                        // To display the equipment required for the exercise in the newsletter
                        .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Select(a => new
            {
                ExerciseVariation = a,
                a.Variation,
                a.Exercise,
                UserExercise = a.Exercise.UserExercises.FirstOrDefault(uv => uv.UserId == user.Id),
                UserExerciseVariation = a.UserExerciseVariations.FirstOrDefault(uv => uv.UserId == user.Id),
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.UserId == user.Id)
            }).AsNoTracking();

        return (await baseQuery.ToListAsync())
            .GroupBy(i => new { i.Exercise.Id, LastSeen = i.UserExercise?.LastSeen ?? DateOnly.MinValue })
            .OrderBy(a => a.Key.LastSeen)
            .Take(count)
            .SelectMany(e => e)
            .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
            .Select(r => new ExerciseDto(Section.None, r.Exercise, r.Variation, r.ExerciseVariation,
                r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                easierVariation: (name: null, reason: null), harderVariation: (name: null, reason: null),
                intensityLevel: null, theme: ExerciseTheme.Main, verbosity: user.Verbosity))
            .ToList();
    }
}
