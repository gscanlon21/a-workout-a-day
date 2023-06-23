﻿using Data.Models.Newsletter;
using Data.Models.User;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Data;
using Data.Data.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Repos;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public partial class NewsletterController : ControllerBase
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
    private readonly UserController _userService;
    private readonly UserRepo _userRepo;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterController(UserRepo userRepo, CoreContext context, UserController userService, IServiceScopeFactory serviceScopeFactory)
    {
        _userRepo = userRepo;
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
        _context = context;
    }
    
    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet("Newsletter")]
    public async Task<NewsletterModel?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null, Client client = Client.None)
    {
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null || user.Disabled
            // User is a debug user. They should see the DebugNewsletter instead.
            || user.Features.HasFlag(Features.Debug))
        {
            return null;
        }

        // User was already sent a newsletter today.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any() && n.Date == Today && n.Client == client)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails)
            // If they're viewing this from the app they can see as many times as they want
            && client == Client.Email)
        {
            return null;
        }

        if (date.HasValue)
        {
            var oldNewsletter = await _context.Newsletters.AsNoTracking()
                .Include(n => n.NewsletterExerciseVariations)
                .Where(n => n.User.Id == user.Id)
                // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
                .Where(n => n.NewsletterExerciseVariations.Any())
                .Where(n => n.Date == date)
                // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created.
                .OrderByDescending(n => n.Id)
                .FirstOrDefaultAsync();

            // A newsletter was found
            if (oldNewsletter != null)
            {
                return await NewsletterOld(user, token, date.Value, oldNewsletter);
            }
            // A newsletter was not found and the date is not one we want to render a new newsletter for
            else if (date != Today)
            {
                return null;
            }
            // Else continue on to render a new newsletter for today
        }

        // User has received an email with a confirmation message, but they did not click to confirm their account.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any()) && user.LastActive == null)
        {
            return null;
        }

        if (user.RestDays.HasFlag(DaysExtensions.FromDate(Today)))
        {
            if (user.SendMobilityWorkouts)
            {
                return await OffDayNewsletter(user, token, client);
            }

            return null;
        }

        return await OnDayNewsletter(user, token, client);
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    [HttpGet("Debug")]
    public async Task<DebugModel?> Debug(string email, string token, Client client = Client.None)
    {
        // The debug user is disabled, not checking that or rest days.
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true);
        if (user == null || user.Disabled || user.RestDays.HasFlag(DaysExtensions.FromDate(Today))
            // User is not a debug user. They should see the Newsletter instead.
            || !user.Features.HasFlag(Features.Debug))
        {
            return null;
        }

        // User was already sent a newsletter today.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any() && n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails))
        {
            return null;
        }

        user.EmailVerbosity = Verbosity.Debug;
        await AddMissingUserExerciseVariationRecords(user);
        var todaysNewsletterRotation = await _userService.GetTodaysNewsletterRotation(user);

        var debugExercises = await GetDebugExercises(user, token, count: 1);

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, client, user.Frequency, needsDeload: false,
            mainExercises: debugExercises
        );
        var equipmentViewModel = new Data.Models.Newsletter.EquipmentModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var viewModel = new DebugModel(user, token)
        {
            AllEquipment = equipmentViewModel,
            DebugExercises = debugExercises,
        };

        await UpdateLastSeenDate(debugExercises);

        //ViewData[ViewData_Newsletter.NeedsDeload] = false;
        return viewModel;
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<NewsletterModel?> OnDayNewsletter(Data.Entities.User.User user, string token, Client client)
    {
        await AddMissingUserExerciseVariationRecords(user);

        (var needsDeload, var timeUntilDeload) = await _userService.CheckNewsletterDeloadStatus(user);
        var todaysNewsletterRotation = await _userService.GetTodaysNewsletterRotation(user);

        // Choose cooldown first, these are the easiest so we want to work variations that can be a part of two or more sections here.
        var cooldownExercises = await GetCooldownExercises(user, todaysNewsletterRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysNewsletterRotation, token,
            // Never work the same variation twice
            excludeVariations: cooldownExercises);

        var coreExercises = await GetCoreExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, lowerIntensity: needsDeload),
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises));

        var functionalExercises = await GetFunctionalExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, needsDeload), todaysNewsletterRotation,
            // Never work the same variation twice
            excludeVariations: cooldownExercises.Concat(warmupExercises).Concat(coreExercises));

        // Lower the intensity to reduce the risk of injury from heavy-weighted isolation exercises.
        var accessoryExercises = await GetAccessoryExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, lowerIntensity: true), todaysNewsletterRotation,
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

        var sportsExercises = await GetSportsExercises(user, token, todaysNewsletterRotation, ToIntensityLevel(user.IntensityLevel, needsDeload), needsDeload,
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

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, client, user.Frequency, needsDeload: needsDeload,
            rehabExercises: rehabExercises,
            warmupExercises: warmupExercises,
            sportsExercises: sportsExercises,
            mainExercises: functionalExercises.Concat(accessoryExercises).Concat(coreExercises).ToList(),
            prehabExercises: prehabExercises,
            cooldownExercises: cooldownExercises
        );

        var userViewModel = new UserNewsletterModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new NewsletterModel(userViewModel, newsletter)
        {
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            MainExercises = functionalExercises.Concat(accessoryExercises).Concat(coreExercises).ToList(),
            SportsExercises = sportsExercises,
            CooldownExercises = cooldownExercises
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
    private async Task<NewsletterModel?> OffDayNewsletter(Data.Entities.User.User user, string token, Client client)
    {
        await AddMissingUserExerciseVariationRecords(user);

        (var needsDeload, var timeUntilDeload) = await _userService.CheckNewsletterDeloadStatus(user);
        var todaysNewsletterRotation = await _userService.GetTodaysNewsletterRotation(user, Frequency.OffDayStretches);

        // Choose cooldown first, these are the easiest so we want to work variations that can be a part of two or more sections here.
        var cooldownExercises = await GetCooldownExercises(user, todaysNewsletterRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysNewsletterRotation, token,
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

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, client, Frequency.OffDayStretches, needsDeload: needsDeload,
            warmupExercises: warmupExercises,
            cooldownExercises: cooldownExercises,
            mainExercises: coreExercises,
            prehabExercises: prehabExercises,
            rehabExercises: rehabExercises
        );
        var userViewModel = new UserNewsletterModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new NewsletterModel(userViewModel, newsletter)
        {
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
    private async Task<NewsletterModel?> NewsletterOld(Data.Entities.User.User user, string token, DateOnly date, Data.Entities.Newsletter.Newsletter newsletter)
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
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Core.Models.Newsletter.Section.Prehab)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
            .ToList();

        var rehabExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Core.Models.Newsletter.Section.Rehab)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
            .ToList();

        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Core.Models.Newsletter.Section.Warmup)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Warmup, token))
            .ToList();

        var mainExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Core.Models.Newsletter.Section.Main)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Main, token))
            .ToList();

        var cooldownExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Core.Models.Newsletter.Section.Cooldown)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Cooldown, token))
            .ToList();

        var sportsExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Core.Models.Newsletter.Section.Sports)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseModel(r, IntensityLevel.Warmup, ExerciseTheme.Other, token))
            .ToList();

        var userViewModel = new UserNewsletterModel(user, token);

        if (newsletter.Frequency == Frequency.OffDayStretches)
        {
            return new NewsletterModel(userViewModel, newsletter)
            {
                PrehabExercises = prehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                RehabExercises = rehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                WarmupExercises = warmupExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                MainExercises = mainExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                CooldownExercises = cooldownExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                SportsExercises = new List<ExerciseModel>()
            };
        }

        var viewModel = new NewsletterModel(userViewModel, newsletter)
        {
            Today = date,
            PrehabExercises = prehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            RehabExercises = rehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            WarmupExercises = warmupExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            MainExercises = mainExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            SportsExercises = sportsExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            CooldownExercises = cooldownExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList()
        };

        return viewModel;
    }

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseModel>> GetDebugExercises(Data.Entities.User.User user, string token, int count = 1)
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
