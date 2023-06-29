﻿using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Data;
using Data.Data.Query;
using Data.Models.Newsletter;
using Data.Models.User;
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
    public async Task<NewsletterModel?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null)
    {
        var user = await _userRepo.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null || user.Disabled)
        {
            return null;
        }

        // User is a debug user. They should see the DebugNewsletter instead.
        if (user.Features.HasFlag(Features.Debug))
        {
            _logger.Log(LogLevel.Information, "Returning debug newsletter for user {Id}", user.Id);
            return await Debug(email, token);
        }

        if (date.HasValue && !user.Features.HasFlag(Features.Demo))
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

        // User has received an email with a confirmation message, but they did not click to confirm their account.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.UserWorkouts.AnyAsync(n => n.UserId == user.Id && n.UserWorkoutExerciseVariations.Any()) && user.LastActive == null)
        {
            _logger.Log(LogLevel.Information, "Returning no newsletter for user {Id}", user.Id);
            return null;
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
    public async Task<NewsletterModel?> Debug(string email, string token)
    {
        // The debug user is disabled, not checking that or rest days.
        var user = await _userRepo.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true);
        if (user == null || user.Disabled || user.RestDays.HasFlag(DaysExtensions.FromDate(Today))
            // User is not a debug user. They should see the Newsletter instead.
            || !user.Features.HasFlag(Features.Debug))
        {
            return null;
        }

        user.Verbosity = Verbosity.Debug;
        await AddMissingUserExerciseVariationRecords(user);
        var todaysWorkoutRotation = await _userRepo.GetTodaysWorkoutRotation(user);

        var debugExercises = await GetDebugExercises(user, token, count: 1);
        var newsletter = await CreateAndAddNewsletterToContext(user, todaysWorkoutRotation, user.Frequency, needsDeload: false,
            mainExercises: debugExercises
        );
        var equipmentViewModel = new EquipmentModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterModel(user, token);
        var viewModel = new NewsletterModel(userViewModel, newsletter)
        {
            Equipment = equipmentViewModel,
            MainExercises = debugExercises,
            PrehabExercises = new List<ExerciseModel>(),
            RehabExercises = new List<ExerciseModel>(),
            SportsExercises = new List<ExerciseModel>(),
            WarmupExercises = new List<ExerciseModel>(),
            CooldownExercises = new List<ExerciseModel>(),
        };

        await UpdateLastSeenDate(debugExercises);

        //ViewData[ViewData_Newsletter.NeedsDeload] = false;
        return viewModel;
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<NewsletterModel?> OnDayNewsletter(Entities.User.User user, string token)
    {
        await AddMissingUserExerciseVariationRecords(user);

        (var needsDeload, var timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);
        var todaysWorkoutRotation = await _userRepo.GetTodaysWorkoutRotation(user);

        // Choose cooldown first, these are the easiest so we want to work variations that can be a part of two or more sections here.
        var cooldownExercises = await GetCooldownExercises(user, todaysWorkoutRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysWorkoutRotation, token,
            // Never work the same variation twice
            excludeVariations: cooldownExercises);

        var coreExercises = await GetCoreExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, lowerIntensity: needsDeload),
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises));

        var functionalExercises = await GetFunctionalExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, needsDeload), todaysWorkoutRotation,
            // Never work the same variation twice
            excludeVariations: cooldownExercises.Concat(warmupExercises).Concat(coreExercises));

        // Lower the intensity to reduce the risk of injury from heavy-weighted isolation exercises.
        var accessoryExercises = await GetAccessoryExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, lowerIntensity: true), todaysWorkoutRotation,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            // sa. exclude all Plank variations if we already worked any Plank variation earlier
            excludeGroups: functionalExercises.Concat(coreExercises),
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            // sa. exclude all Plank variations if we already worked any Plank variation earlier
            excludeExercises: functionalExercises.Concat(coreExercises),
            // Never work the same variation twice
            excludeVariations: functionalExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(coreExercises),
            // Unset muscles that have already been worked by the functional exercises.
            workedMusclesDict: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                // Weight secondary muscles as half.
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles).ToDictionary(kv => kv.Key, kv => kv.Value / 2)
            ));

        var sportsExercises = await GetSportsExercises(user, token, todaysWorkoutRotation, ToIntensityLevel(user.IntensityLevel, needsDeload), needsDeload,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            // sa. exclude all Plank variations if we already worked any Plank variation earlier
            excludeGroups: functionalExercises.Concat(accessoryExercises).Concat(coreExercises),
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            // sa. exclude all Plank variations if we already worked any Plank variation earlier
            excludeExercises: functionalExercises.Concat(accessoryExercises).Concat(coreExercises),
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises).Concat(functionalExercises).Concat(accessoryExercises));

        var rehabExercises = await GetRecoveryExercises(user, token);
        // Grab strengthening prehab exercises.
        // Not using a strengthening intensity level because we don't want these tracked by the weekly muscle volume tracker.
        var prehabExercises = await GetPrehabExercises(user, token, needsDeload, IntensityLevel.Recovery, strengthening: true,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises).Concat(functionalExercises).Concat(accessoryExercises).Concat(sportsExercises));

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysWorkoutRotation, user.Frequency, needsDeload: needsDeload,
            rehabExercises: rehabExercises,
            warmupExercises: warmupExercises,
            sportsExercises: sportsExercises,
            mainExercises: functionalExercises.Concat(accessoryExercises).Concat(coreExercises).ToList(),
            prehabExercises: prehabExercises,
            cooldownExercises: cooldownExercises
        );

        var equipmentViewModel = new EquipmentModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new NewsletterModel(userViewModel, newsletter)
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
    private async Task<NewsletterModel?> OffDayNewsletter(Entities.User.User user, string token)
    {
        await AddMissingUserExerciseVariationRecords(user);

        (var needsDeload, var timeUntilDeload) = await _userRepo.CheckNewsletterDeloadStatus(user);
        var todaysWorkoutRotation = await _userRepo.GetTodaysWorkoutRotation(user, Frequency.OffDayStretches);

        // Choose cooldown first, these are the easiest so we want to work variations that can be a part of two or more sections here.
        var cooldownExercises = await GetCooldownExercises(user, todaysWorkoutRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysWorkoutRotation, token,
            // Never work the same variation twice
            excludeVariations: cooldownExercises);

        var coreExercises = await GetCoreExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, lowerIntensity: true),
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises));

        var rehabExercises = await GetRecoveryExercises(user, token);
        // Grab stretching prehab exercises
        var prehabExercises = await GetPrehabExercises(user, token, needsDeload, IntensityLevel.Cooldown, strengthening: false,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises));

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysWorkoutRotation, Frequency.OffDayStretches, needsDeload: needsDeload,
            warmupExercises: warmupExercises,
            cooldownExercises: cooldownExercises,
            mainExercises: coreExercises,
            prehabExercises: prehabExercises,
            rehabExercises: rehabExercises
        );

        var equipmentViewModel = new EquipmentModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new NewsletterModel(userViewModel, newsletter)
        {
            Equipment = equipmentViewModel,
            MainExercises = coreExercises,
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            CooldownExercises = cooldownExercises,
            SportsExercises = new List<ExerciseModel>()
        };

        // Refresh these exercises every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(prehabExercises).Concat(rehabExercises));

        return viewModel;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter based on a date.
    /// </summary>
    private async Task<NewsletterModel?> NewsletterOld(Entities.User.User user, string token, DateOnly date, Entities.Newsletter.UserWorkout newsletter)
    {
        await AddMissingUserExerciseVariationRecords(user);

        // Too many things can go wrong if the newsletter is too old. Token expired; Exercises since been disabled;
        if (newsletter == null || date < Today.AddMonths(-1))
        {
            return null;
        }

        var prehabExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.UserWorkoutExerciseVariations
                    .Where(nv => nv.Section == Section.Prehab)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
            .ToList();

        var rehabExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.UserWorkoutExerciseVariations
                    .Where(nv => nv.Section == Section.Rehab)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
            .ToList();

        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.UserWorkoutExerciseVariations
                    .Where(nv => nv.Section == Section.Warmup)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Warmup, token))
            .ToList();

        var mainExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.UserWorkoutExerciseVariations
                    .Where(nv => nv.Section == Section.Main)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Main, token))
            .ToList();

        var cooldownExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.UserWorkoutExerciseVariations
                    .Where(nv => nv.Section == Section.Cooldown)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Cooldown, token))
            .ToList();

        var sportsExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.UserWorkoutExerciseVariations
                    .Where(nv => nv.Section == Section.Sports)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, IntensityLevel.Warmup, ExerciseTheme.Other, token))
            .ToList();

        var equipmentViewModel = new EquipmentModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterModel(user, token);
        return new NewsletterModel(userViewModel, newsletter)
        {
            Today = date,
            Equipment = equipmentViewModel,
            PrehabExercises = prehabExercises.OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            RehabExercises = rehabExercises.OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            WarmupExercises = warmupExercises.OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            MainExercises = mainExercises.OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            SportsExercises = sportsExercises.OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            CooldownExercises = cooldownExercises.OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList()
        };
    }

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseModel>> GetDebugExercises(Entities.User.User user, string token, int count = 1)
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
            .Select(r => new ExerciseModel(user, r.Exercise, r.Variation, r.ExerciseVariation,
                r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                easierVariation: null, harderVariation: null,
                intensityLevel: null, Theme: ExerciseTheme.Extra, token: token))
            .ToList();
    }
}
