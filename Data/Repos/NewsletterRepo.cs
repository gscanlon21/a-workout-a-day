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
using Data.Entities.Newsletter;
using Data.Entities.User;
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

    public async Task<IList<Entities.Footnote.Footnote>> GetFootnotes(string email, string token, int count = 1, FootnoteType ofType = FootnoteType.Bottom)
    {
        var user = await _userRepo.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);

        var footnotes = await _context.Footnotes
            // Has any flag
            .Where(f => (f.Type & ofType) != 0)
            .Where(f => !f.UserId.HasValue || f.User == user)
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return footnotes;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    public async Task<NewsletterDto?> Newsletter(string email, string token, DateOnly? date = null)
    {
        var user = await _userRepo.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null)
        {
            return null;
        }

        _logger.Log(LogLevel.Information, "Building newsletter for user {Id}", user.Id);
        await AddMissingUserExerciseVariationRecords(user);

        // Is the user requesting an old newsletter?
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
            // A newsletter was not found and the date is not one we want to render a new newsletter for.
            else if (date != Today)
            {
                _logger.Log(LogLevel.Information, "Returning no newsletter for user {Id}", user.Id);
                return null;
            }
            // Else continue on to render a new newsletter for today.
        }

        // Context may be null on rest days.
        var context = await BuildWorkoutContext(user, token);
        if (context == null)
        {
            // See if a previous workout exists, we send that back down so the app doesn't render nothing on rest days.
            var currentWorkout = await _userRepo.GetCurrentWorkout(user);
            if (currentWorkout == null)
            {
                _logger.Log(LogLevel.Information, "Returning no newsletter for user {Id}", user.Id);
                return null;
            }

            _logger.Log(LogLevel.Information, "Returning current newsletter for user {Id}", user.Id);
            return await NewsletterOld(user, token, currentWorkout.Date, currentWorkout);
        }

        // User is a debug user. They should see the DebugNewsletter instead.
        if (user.Features.HasFlag(Features.Debug))
        {
            _logger.Log(LogLevel.Information, "Returning debug newsletter for user {Id}", user.Id);
            return await Debug(context);
        }

        // Current day should be a mobility workout.
        if (context.Frequency == Frequency.OffDayStretches)
        {
            _logger.Log(LogLevel.Information, "Returning off day newsletter for user {Id}", user.Id);
            return await OffDayNewsletter(context);
        }

        // Current day should be a strengthening workout.
        _logger.Log(LogLevel.Information, "Returning on day newsletter for user {Id}", user.Id);
        return await OnDayNewsletter(context);
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    internal async Task<NewsletterDto?> Debug(WorkoutContext context)
    {
        context.User.Verbosity = Verbosity.Debug;
        var debugExercises = await GetDebugExercises(context.User, count: 1);
        var newsletter = await CreateAndAddNewsletterToContext(context, exercises: debugExercises);
        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), context.User.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(context);
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

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<NewsletterDto?> OnDayNewsletter(WorkoutContext context)
    {
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

        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), context.User.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(context);
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
            refreshAfter: StartOfWeek.AddDays(7 * context.User.RefreshFunctionalEveryXWeeks));
        // Accessory exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: accessoryExercises,
            refreshAfter: StartOfWeek.AddDays(7 * context.User.RefreshAccessoryEveryXWeeks));
        // Other exercises. Refresh every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(warmupExercises).Concat(cooldownExercises).Concat(prehabExercises).Concat(rehabExercises).Concat(sportsExercises));

        return viewModel;
    }

    /// <summary>
    /// The mobility/stretch newsletter for days off strength training.
    /// </summary>
    private async Task<NewsletterDto?> OffDayNewsletter(WorkoutContext context)
    {
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

        var equipmentViewModel = new EquipmentDto(_context.Equipment.Where(e => e.DisabledReason == null), context.User.UserEquipments.Select(eu => eu.Equipment));
        var userViewModel = new UserNewsletterDto(context);
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
    private async Task<NewsletterDto?> NewsletterOld(User user, string token, DateOnly date, UserWorkout newsletter)
    {
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
}
