using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.Extensions;
using Web.Code.ViewData;
using Web.Data;
using Web.Data.Query;
using Web.Models.Exercise;
using Web.Models.User;
using Web.Services;
using Web.ViewModels.Newsletter;
using Web.ViewModels.User;

namespace Web.Controllers.Newsletter;

[Route("n", Order = 1)]
[Route("newsletter", Order = 2)]
public partial class NewsletterController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Newsletter";

    private readonly CoreContext _context;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UserService _userService;

    public NewsletterController(CoreContext context, UserService userService, IServiceScopeFactory serviceScopeFactory) : base()
    {
        _context = context;
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}/{{date}}", Order = 1)]
    [Route($"{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 2)]
    [Route("demo", Order = 3)]
    public async Task<IActionResult> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null)
    {
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null || user.Disabled
            // User is a debug user. They should see the DebugNewsletter instead.
            || user.Features.HasFlag(Features.Debug))
        {
            return NoContent();
        }

        if (date.HasValue)
        {
            return await NewsletterOld(user, token, date.Value);
        }

        // User was already sent a newsletter today.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any() && n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails))
        {
            return NoContent();
        }

        // User has received an email with a confirmation message, but they did not click to confirm their account.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any()) && user.LastActive == null)
        {
            return NoContent();
        }

        if (user.RestDays.HasFlag(DaysExtensions.FromDate(Today)))
        {
            if (user.SendMobilityWorkouts)
            {
                return await OffDayNewsletter(user, token);
            }

            return NoContent();
        }

        return await OnDayNewsletter(user, token);
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<IActionResult> OnDayNewsletter(Entities.User.User user, string token)
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
            // Unset muscles that have already been worked by the functional exercises
            workedMusclesDict: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles),
            // Unset muscles that have already been worked by the functional exercises
            secondaryWorkedMusclesDict: functionalExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles));

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
        // Lower the intensity to reduce the risk of injury from heavy-weighted isolation exercises.
        var prehabExercises = await GetPrehabExercises(user, token, needsDeload, ToIntensityLevel(user.IntensityLevel, lowerIntensity: true), strengthening: true,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises).Concat(functionalExercises).Concat(accessoryExercises).Concat(sportsExercises));

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, user.Frequency, needsDeload: needsDeload,
            rehabExercises: rehabExercises,
            warmupExercises: warmupExercises, 
            sportsExercises: sportsExercises,
            mainExercises: functionalExercises.Concat(accessoryExercises).Concat(coreExercises).ToList(),
            prehabExercises: prehabExercises,
            cooldownExercises: cooldownExercises
        );

        var userViewModel = new UserNewsletterViewModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new NewsletterViewModel(userViewModel, newsletter)
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

        ViewData[ViewData_Newsletter.NeedsDeload] = needsDeload;
        return View(nameof(Newsletter), viewModel);
    }

    /// <summary>
    /// The mobility/stretch newsletter for days off strength training.
    /// </summary>
    private async Task<IActionResult> OffDayNewsletter(Entities.User.User user, string token)
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
        var prehabExercises = await GetPrehabExercises(user, token, needsDeload, IntensityLevel.Cooldown, strengthening: false,
            // Never work the same variation twice
            excludeVariations: warmupExercises.Concat(cooldownExercises).Concat(coreExercises));

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, Frequency.OffDayStretches, needsDeload: needsDeload,
            warmupExercises: warmupExercises,
            cooldownExercises: cooldownExercises,
            mainExercises: coreExercises,
            prehabExercises: prehabExercises,
            rehabExercises: rehabExercises
        );
        var userViewModel = new UserNewsletterViewModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new OffDayNewsletterViewModel(userViewModel, newsletter)
        {
            CoreExercises = coreExercises,
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            MobilityExercises = warmupExercises,
            FlexibilityExercises = cooldownExercises
        };

        // Refresh these exercises every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(prehabExercises).Concat(rehabExercises));

        ViewData[ViewData_Newsletter.NeedsDeload] = needsDeload;
        return View(nameof(OffDayNewsletter), viewModel);
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter based on a date.
    /// </summary>
    private async Task<IActionResult> NewsletterOld(Entities.User.User user, string token, DateOnly date)
    {
        var newsletter = await _context.Newsletters.AsNoTracking()
            .Include(n => n.NewsletterExerciseVariations)
            .Where(n => n.User.Id == user.Id)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.NewsletterExerciseVariations.Any())
            .Where(n => n.Date == date)
            // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created.
            .OrderByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        // Too many things can go wrong if the newsletter is too old. Token expired; Exercises since been disabled;
        if (newsletter == null || date < Today.AddMonths(-1))
        {
            return NotFound();
        }

        var prehabExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Models.Newsletter.Section.Prehab)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
            .ToList();

        var rehabExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Models.Newsletter.Section.Rehab)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
            .ToList();

        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Models.Newsletter.Section.Warmup)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Warmup, token))
            .ToList();

        var mainExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Models.Newsletter.Section.Main)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Main, token))
            .ToList();

        var cooldownExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Models.Newsletter.Section.Cooldown)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Cooldown, token))
            .ToList();

        var sportsExercises = (await new QueryBuilder(_context)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true)
            .WithExercises(options =>
            {
                options.ExerciseVariationIds = newsletter.NewsletterExerciseVariations
                    .Where(nv => nv.Section == Models.Newsletter.Section.Sports)
                    .Select(nv => nv.ExerciseVariationId)
                    .ToList();
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Other, token))
            .ToList();

        var userViewModel = new UserNewsletterViewModel(user, token);

        ViewData[ViewData_Newsletter.NeedsDeload] = newsletter.IsDeloadWeek;
        if (newsletter.Frequency == Frequency.OffDayStretches)
        {
            return View(nameof(OffDayNewsletter), new OffDayNewsletterViewModel(userViewModel, newsletter)
            {
                PrehabExercises = prehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                RehabExercises = rehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                MobilityExercises = warmupExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                CoreExercises = mainExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                FlexibilityExercises = cooldownExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList()
            });
        }

        return View(nameof(Newsletter), new NewsletterViewModel(userViewModel, newsletter)
        {
            PrehabExercises = prehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            RehabExercises = rehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            WarmupExercises = warmupExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            MainExercises = mainExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            SportsExercises = sportsExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
            CooldownExercises = cooldownExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList()
        });
    }
}
