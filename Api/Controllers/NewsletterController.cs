using Api.ViewModels.Newsletter;
using Api.ViewModels.User;
using Core.Code.Extensions;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Data;
using Data.Data.Query;
using Data.Entities.Exercise;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsletterController : ControllerBase
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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterController(CoreContext context, UserController userService, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
        _context = context;
    }


    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetDebugExercises(Data.Entities.User.User user, string token, int count = 1)
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
            .Select(r => new ExerciseViewModel(user, r.Exercise, r.Variation, r.ExerciseVariation,
                r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                easierVariation: null, harderVariation: null,
                intensityLevel: null, Theme: ExerciseTheme.Extra, token: token))
            .ToList();
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    //[HttpGet(Name = "Debug")]
    private async Task<object?> Debug(string email, string token)
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

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, user.Frequency, needsDeload: false,
            mainExercises: debugExercises
        );
        var equipmentViewModel = new Api.ViewModels.Newsletter.EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var viewModel = new DebugViewModel(user, token)
        {
            AllEquipment = equipmentViewModel,
            DebugExercises = debugExercises,
        };

        await UpdateLastSeenDate(debugExercises);

        //ViewData[ViewData_Newsletter.NeedsDeload] = false;
        return viewModel;
    }



    /// <summary>
    /// The exercise query runner requires UserExercise/UserExerciseVariation/UserVariation records to have already been made.
    /// There is a small chance for a race-condition if Exercise/ExerciseVariation/Variation records are added after these run in.
    /// I'm not concerned about that possiblity because the data changes infrequently, and the newsletter will resend with the next trigger (twice-hourly).
    /// </summary>
    private async Task AddMissingUserExerciseVariationRecords(Data.Entities.User.User user)
    {
        // When EF Core allows batching seperate queries, refactor this.
        var missingUserExercises = await _context.Exercises.TagWithCallSite()
            .Where(e => !_context.UserExercises.Where(ue => ue.UserId == user.Id).Select(ue => ue.ExerciseId).Contains(e.Id))
            .Select(e => new { e.Id, e.Proficiency })
            .ToListAsync();

        var missingUserExerciseVariationIds = await _context.ExerciseVariations.TagWithCallSite()
            .Where(e => !_context.UserExerciseVariations.Where(ue => ue.UserId == user.Id).Select(ue => ue.ExerciseVariationId).Contains(e.Id))
            .Select(ev => ev.Id)
            .ToListAsync();

        var missingUserVariationIds = await _context.Variations.TagWithCallSite()
            .Where(e => !_context.UserVariations.Where(ue => ue.UserId == user.Id).Select(ue => ue.VariationId).Contains(e.Id))
            .Select(v => v.Id)
            .ToListAsync();

        // Add missing User* records
        _context.UserExercises.AddRange(missingUserExercises.Select(e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserExercise.MinUserProgression : e.Proficiency }));
        _context.UserExerciseVariations.AddRange(missingUserExerciseVariationIds.Select(evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id }));
        _context.UserVariations.AddRange(missingUserVariationIds.Select(vId => new UserVariation() { VariationId = vId, UserId = user.Id }));

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    private async Task<Data.Entities.Newsletter.Newsletter> CreateAndAddNewsletterToContext(Data.Entities.User.User user, NewsletterRotation newsletterRotation, Frequency frequency, bool needsDeload,
        IList<ExerciseViewModel>? rehabExercises = null,
        IList<ExerciseViewModel>? warmupExercises = null,
        IList<ExerciseViewModel>? sportsExercises = null,
        IList<ExerciseViewModel>? mainExercises = null,
        IList<ExerciseViewModel>? prehabExercises = null,
        IList<ExerciseViewModel>? cooldownExercises = null)
    {
        var newsletter = new Data.Entities.Newsletter.Newsletter(Today, user, newsletterRotation, frequency, isDeloadWeek: needsDeload);
        _context.Newsletters.Add(newsletter); // Sets the newsletter.Id after changes are saved.
        await _context.SaveChangesAsync();

        if (rehabExercises != null)
        {
            for (var i = 0; i < rehabExercises.Count; i++)
            {
                var exercise = rehabExercises[i];
                _context.NewsletterExerciseVariations.Add(new NewsletterExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Rehab
                });
            }
        }
        if (warmupExercises != null)
        {
            for (var i = 0; i < warmupExercises.Count; i++)
            {
                var exercise = warmupExercises[i];
                _context.NewsletterExerciseVariations.Add(new NewsletterExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Warmup
                });
            }
        }
        if (sportsExercises != null)
        {
            for (var i = 0; i < sportsExercises.Count; i++)
            {
                var exercise = sportsExercises[i];
                _context.NewsletterExerciseVariations.Add(new NewsletterExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Sports
                });
            }
        }
        if (mainExercises != null)
        {
            for (var i = 0; i < mainExercises.Count; i++)
            {
                var exercise = mainExercises[i];
                _context.NewsletterExerciseVariations.Add(new NewsletterExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Main
                });
            }
        }
        if (prehabExercises != null)
        {
            for (var i = 0; i < prehabExercises.Count; i++)
            {
                var exercise = prehabExercises[i];
                _context.NewsletterExerciseVariations.Add(new NewsletterExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Prehab
                });
            }
        }
        if (cooldownExercises != null)
        {
            for (var i = 0; i < cooldownExercises.Count; i++)
            {
                var exercise = cooldownExercises[i];
                _context.NewsletterExerciseVariations.Add(new NewsletterExerciseVariation(newsletter, exercise.ExerciseVariation)
                {
                    IntensityLevel = exercise.IntensityLevel,
                    Order = i,
                    Section = Section.Cooldown
                });
            }
        }

        await _context.SaveChangesAsync();
        return newsletter;
    }

    /// <summary>
    /// 
    /// </summary>
    private IntensityLevel ToIntensityLevel(IntensityLevel userIntensityLevel, bool lowerIntensity = false)
    {
        if (lowerIntensity)
        {
            return userIntensityLevel switch
            {
                IntensityLevel.Light => IntensityLevel.Endurance,
                IntensityLevel.Medium => IntensityLevel.Light,
                IntensityLevel.Heavy => IntensityLevel.Medium,
                _ => throw new NotImplementedException()
            };
        }

        return userIntensityLevel switch
        {
            IntensityLevel.Light => IntensityLevel.Light,
            IntensityLevel.Medium => IntensityLevel.Medium,
            IntensityLevel.Heavy => IntensityLevel.Heavy,
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    ///     Updates the last seen date of the exercise by the user.
    /// </summary>
    /// <param name="refreshAfter">
    ///     When set and the date is > Today, hold off on refreshing the LastSeen date so that we see the same exercises in each workout.
    /// </param>
    private async Task UpdateLastSeenDate(IEnumerable<ExerciseViewModel> exercises, DateOnly? refreshAfter = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (exerciseDict[exercise].UserExercise!.RefreshAfter == null || Today >= exerciseDict[exercise].UserExercise!.RefreshAfter)
            {
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (exerciseDict[exercise].UserExercise!.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > Today)
                {
                    exerciseDict[exercise].UserExercise!.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    exerciseDict[exercise].UserExercise!.RefreshAfter = null;
                    exerciseDict[exercise].UserExercise!.LastSeen = Today;
                }
                scopedCoreContext.UserExercises.Update(exerciseDict[exercise].UserExercise!);
            }
        }

        var exerciseVariationDict = exercises.DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            // >= so that today is the last day seeing the same exercises and tomorrow the exercises will refresh.
            if (exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter == null || Today >= exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter)
            {
                // If refresh after is today, we want to see a different exercises tomorrow so update the last seen date.
                if (exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter == null && refreshAfter.HasValue && refreshAfter.Value > Today)
                {
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter = refreshAfter.Value;
                }
                else
                {
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.RefreshAfter = null;
                    exerciseVariationDict[exerciseVariation].UserExerciseVariation!.LastSeen = Today;
                }
                scopedCoreContext.UserExerciseVariations.Update(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
        }

        await scopedCoreContext.SaveChangesAsync();
    }


    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [HttpGet("Newsletter")]
    public async Task<NewsletterViewModel?> Newsletter(string email = "demo@aworkoutaday.com", string token = "00000000-0000-0000-0000-000000000000", DateOnly? date = null, string? format = null)
    {
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null || user.Disabled
            // User is a debug user. They should see the DebugNewsletter instead.
            || user.Features.HasFlag(Features.Debug))
        {
            return null;
        }

        if (date.HasValue)
        {
            return await NewsletterOld(user, token, date.Value, format);
        }

        // User was already sent a newsletter today.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterExerciseVariations.Any() && n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails))
        {
            return null;
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
                return await OffDayNewsletter(user, token, format);
            }

            return null;
        }

        return await OnDayNewsletter(user, token, format);
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<NewsletterViewModel?> OnDayNewsletter(Data.Entities.User.User user, string token, string? format)
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

        return viewModel;
    }

    /// <summary>
    /// The mobility/stretch newsletter for days off strength training.
    /// </summary>
    private async Task<NewsletterViewModel?> OffDayNewsletter(Data.Entities.User.User user, string token, string? format)
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
        var viewModel = new NewsletterViewModel(userViewModel, newsletter)
        {
            MainExercises = coreExercises,
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            CooldownExercises = cooldownExercises,
            SportsExercises = new List<ExerciseViewModel>()
        };

        // Refresh these exercises every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(prehabExercises).Concat(rehabExercises));

        return viewModel;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter based on a date.
    /// </summary>
    private async Task<NewsletterViewModel?> NewsletterOld(Data.Entities.User.User user, string token, DateOnly date, string? format)
    {
        await AddMissingUserExerciseVariationRecords(user);

        var newsletter = await _context.Newsletters.AsNoTracking()
            .Include(n => n.NewsletterExerciseVariations)
            .Where(n => n.User.Id == user.Id)
            // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
            .Where(n => n.NewsletterExerciseVariations.Any())
            .Where(n => n.Date == date)
            // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created.
            .OrderByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        // Go back to render a new newsletter for today.
        if (newsletter == null && date == Today)
        {
            return await Newsletter(user.Email, token, date: null, format: format);
        }

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
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
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
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Extra, token))
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
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Warmup, token))
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
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Main, token))
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
            .Select(r => new ExerciseViewModel(r, newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == r.ExerciseVariation.Id).IntensityLevel.GetValueOrDefault(), ExerciseTheme.Cooldown, token))
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
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Other, token))
            .ToList();

        var userViewModel = new UserNewsletterViewModel(user, token);

        if (newsletter.Frequency == Frequency.OffDayStretches)
        {
            return new NewsletterViewModel(userViewModel, newsletter)
            {
                PrehabExercises = prehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                RehabExercises = rehabExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                WarmupExercises = warmupExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                MainExercises = mainExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                CooldownExercises = cooldownExercises.OrderBy(e => newsletter.NewsletterExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order).ToList(),
                SportsExercises = new List<ExerciseViewModel>()
            };
        }

        var viewModel = new NewsletterViewModel(userViewModel, newsletter)
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



    #region Warmup Exercises

    /// <summary>
    /// Returns a list of warmup exercises.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetWarmupExercises(Data.Entities.User.User user, NewsletterRotation newsletterRotation, string token,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            .WithExerciseType(ExerciseType.Stretching, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            // Not checking .OnlyWeights(false) because some warmup exercises require weights to perform, such as Plate/Kettlebell Halos and Hip Weight Shift.
            //.WithOnlyWeights(false)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
            .ToList();

        // Get the heart rate up. Can work any muscle.
        // Ideal is 2-5 minutes. We want to provide at least 2x60s exercises.
        var warmupCardio = (await new QueryBuilder(_context)
            .WithUser(user)
            // We just want to get the blood flowing. It doesn't matter what muscles these work.
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithExerciseType(ExerciseType.CardiovasularTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.CardiovasularTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Endurance)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Take(2)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
            .ToList();

        return warmupExercises.Concat(warmupCardio).ToList();
    }

    #endregion
    #region Cooldown Exercises

    /// <summary>
    /// Returns a list of cooldown exercises.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetCooldownExercises(Data.Entities.User.User user, NewsletterRotation newsletterRotation, string token,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        var stretches = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // These are static stretches so only look at stretched muscles
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = newsletterRotation.IsFullBody ? 3 : 2;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Stretching, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Cooldown, ExerciseTheme.Cooldown, token))
            .ToList();

        var mindfulness = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.Mindfulness, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.Mindfulness | ExerciseType.Stretching;
            })
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Cooldown, ExerciseTheme.Cooldown, token))
            .ToList();

        return stretches.Concat(mindfulness).ToList();
    }

    #endregion
    #region Recovery Exercises

    /// <summary>
    /// Returns a list of recovery exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetRecoveryExercises(Data.Entities.User.User user, string token)
    {
        if (user.RehabFocus.As<MuscleGroups>() == MuscleGroups.None)
        {
            return new List<ExerciseViewModel>();
        }

        var rehabMain = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithJoints(user.RehabFocus.As<Joints>())
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
            })
            .WithExcludeExercises(x => { })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Extra, token))
            .ToList();

        var rehabCooldown = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithJoints(user.RehabFocus.As<Joints>())
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeVariations(rehabMain?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Extra, token))
            .ToList();

        var rehabWarmup = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithJoints(user.RehabFocus.As<Joints>())
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeVariations(rehabCooldown?.Select(vm => vm.Variation));
                x.AddExcludeVariations(rehabMain?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Extra, token))
            .ToList();

        return rehabWarmup.Concat(rehabMain).Concat(rehabCooldown).ToList();
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetSportsExercises(Data.Entities.User.User user, string token, NewsletterRotation newsletterRotation, IntensityLevel intensityLevel, bool needsDeload,
         IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.SportsFocus == SportsFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        var sportsPlyo = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroupsSansCore, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.SportsTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.SportsTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Strength | ExerciseFocus.Power | ExerciseFocus.Endurance | ExerciseFocus.Stability | ExerciseFocus.Agility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(user.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Other, token));

        var sportsStrength = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroupsSansCore, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.SportsTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.SportsTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Strength | ExerciseFocus.Power | ExerciseFocus.Endurance | ExerciseFocus.Stability | ExerciseFocus.Agility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(user.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic | MuscleMovement.Isometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(sportsPlyo.Select(vm => vm.Exercise));
                x.AddExcludeVariations(sportsPlyo.Select(vm => vm.Variation));
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Other, token));

        return sportsPlyo.Concat(sportsStrength).ToList();
    }

    #endregion
    #region Core Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetCoreExercises(Data.Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.Core, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // We don't want to work just one core muscle at a time because that is prime for muscle imbalances
                x.AtLeastXMusclesPerExercise = 2;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.ResistanceTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetPrehabExercises(Data.Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, bool strengthening,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        var results = new List<ExerciseViewModel>();
        foreach (var eVal in EnumExtensions.GetValuesExcluding32(PrehabFocus.None, PrehabFocus.All).Where(v => user.PrehabFocus.HasFlag(v)))
        {
            results.AddRange((await new QueryBuilder(_context)
                .WithUser(user)
                .WithJoints(eVal.As<Joints>())
                .WithMuscleGroups(eVal.As<MuscleGroups>(), x =>
                {
                    // Try to work isolation exercises (for muscle groups, not joints)? x.AtMostXUniqueMusclesPerExercise = 1; Reverse the loop in the QueryRunner and increment.
                    x.MuscleTarget = strengthening ? vm => vm.Variation.StrengthMuscles
                                                   : vm => vm.Variation.StretchMuscles;
                    x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = needsDeload;
                })
                .WithExerciseType(ExerciseType.InjuryPrevention | ExerciseType.BalanceTraining)
                // Train mobility in total.
                .WithExerciseFocus(strengthening
                    ? ExerciseFocus.Stability | ExerciseFocus.Strength
                    : ExerciseFocus.Flexibility)
                .WithExcludeExercises(x =>
                {
                    x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                    x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                    x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    x.AddExcludeVariations(results?.Select(vm => vm.Variation));
                })
                // No cardio, strengthening exercises only
                .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                .Build()
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Extra, token))
            );
        }

        return results;
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Returns a list of functional exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetFunctionalExercises(Data.Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, NewsletterRotation newsletterRotation,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithMovementPatterns(newsletterRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.ResistanceTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Strength)
            // No isometric, we're wanting to work functional movements. No plyometric, those are too intense for strength training outside of sports focus.
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithOrderBy(OrderBy.MuscleTarget)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetAccessoryExercises(Data.Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, NewsletterRotation newsletterRotation,
        IEnumerable<ExerciseViewModel> excludeGroups, IEnumerable<ExerciseViewModel> excludeExercises, IEnumerable<ExerciseViewModel> excludeVariations, IDictionary<MuscleGroups, int> workedMusclesDict)
    {
        // If the user expects accessory exercises and has a deload week, don't show them the accessory exercises.
        // User is new to fitness? Don't add additional accessory exercises to the core set.
        if (user.IsNewToFitness || needsDeload)
        {
            return new List<ExerciseViewModel>();
        }

        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Only target muscles of our current rotation's muscle groups.
            .Where(mg => newsletterRotation.MuscleGroups.HasFlag(mg))
            // Base 1 target for each muscle group. If we've already worked this muscle, reduce the muscle target volume.
            .ToDictionary(mg => mg, mg => 1 - (workedMusclesDict.TryGetValue(mg, out int workedAmt) ? workedAmt : 0));

        // Adjustments to the muscle groups to reduce muscle imbalances.
        var weeklyMuscles = await _userService.GetWeeklyMuscleVolume(user, weeks: Math.Max(Data.Entities.User.User.DeloadAfterEveryXWeeksDefault, user.DeloadAfterEveryXWeeks));
        if (weeklyMuscles != null)
        {
            foreach (var key in muscleTargets.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (weeklyMuscles[key].HasValue)
                {
                    var targetRange = user.UserMuscles.Cast<UserMuscle?>().FirstOrDefault(um => um?.MuscleGroup == key)?.Range ?? UserController.MuscleTargets[key];

                    // We work this muscle group too often
                    if (weeklyMuscles[key] > targetRange.End.Value)
                    {
                        muscleTargets[key] = muscleTargets[key] - Math.Max(1, (weeklyMuscles[key].GetValueOrDefault() - targetRange.End.Value) / Proficiency.TargetVolumePerExercise);
                    }
                    // We don't work this muscle group often enough
                    else if (weeklyMuscles[key] < targetRange.Start.Value)
                    {
                        muscleTargets[key] = muscleTargets[key] + Math.Max(1, (targetRange.Start.Value - weeklyMuscles[key].GetValueOrDefault()) / Proficiency.TargetVolumePerExercise);
                    }
                }
            }
        }

        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.None, x =>
            {
                x.MuscleTargets = muscleTargets;
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.AtLeastXUniqueMusclesPerExercise = newsletterRotation.IsFullBody ? 3 : 2;
                x.SecondaryMuscleTarget = vm => vm.Variation.SecondaryMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.ResistanceTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations.Select(vm => vm.Variation));
            })
            // Leave movement patterns to the first part of the main section - so we don't work a pull on a push day.
            .WithMovementPatterns(MovementPattern.None)
            // No plyometric, leave those to sports-focus or warmup-cardio
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithOrderBy(OrderBy.CoreLast)
            .Build()
            .Query())
            .Select(e => new ExerciseViewModel(e, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
}
