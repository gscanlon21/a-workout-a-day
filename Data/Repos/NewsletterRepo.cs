using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Entities.Footnote;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.Repos;

public partial class NewsletterRepo
{
    private readonly UserRepo _userRepo;
    private readonly CoreContext _context;
    private readonly ILogger<NewsletterRepo> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterRepo(ILogger<NewsletterRepo> logger, CoreContext context, UserRepo userRepo, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<IList<Footnote>> GetFootnotes(string? email, string? token, int count = 1)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        var footnotes = await _context.Footnotes
            // Apply the user's footnote type preferences. Has any flag.
            .Where(f => user == null || (f.Type & user.FootnoteType) != 0)
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        return footnotes;
    }

    public async Task<IList<UserFootnote>> GetUserFootnotes(string? email, string? token, int count = 1)
    {
        var user = await _userRepo.GetUserStrict(email, token, allowDemoUser: true);
        if (!user.FootnoteType.HasFlag(FootnoteType.Custom))
        {
            return [];
        }

        // GetValueOrDefault can't be translated by EF Core.
        var footnotes = await _context.UserFootnotes
            .Where(f => f.Type == FootnoteType.Custom)
            .Where(f => f.UserId == user.Id)
            // Keep the same footnotes over the course of a day.
            .OrderByDescending(f => f.LastSeen == DateHelpers.Today)
            .ThenBy(f => f.LastSeen.HasValue ? f.LastSeen : DateOnly.MinValue)
            .ThenBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

        footnotes.ForEach(f => f.LastSeen = DateHelpers.Today);
        await _context.SaveChangesAsync();
        return footnotes;
    }

    public async Task<NewsletterDto?> Newsletter(string email, string token, DateOnly? date = null)
    {
        var user = await _userRepo.GetUserStrict(email, token, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (!user.LastActive.HasValue) { return null; }
        return await Newsletter(user, token, date);
    }

    /// <summary>
    /// Root route for building out the workout routine newsletter.
    /// </summary>
    public async Task<NewsletterDto?> Newsletter(User user, string token, DateOnly? date = null)
    {
        date ??= user.TodayOffset;

        _logger.Log(LogLevel.Information, "User {Id}: Building workout for {date}", user.Id, date);
        Logs.AppendLog(user, $"{date}: Building workout with options WorkoutsPerWeek={user.WorkoutsDays}");

        // Is the user requesting an old newsletter?
        var oldNewsletters = await _context.UserWorkouts.AsNoTracking()
            .IgnoreQueryFilters().TagWithCallSite()
            .Include(n => n.UserWorkoutVariations)
            .Where(n => n.UserId == user.Id)
            .Where(n => n.Date == date)
            // Checking for variations because we create a dummy workout to advance the workout split.
            .Where(n => n.UserWorkoutVariations.Any())
            .OrderByDescending(n => n.Id)
            .ToListAsync();

        // Always send a new newsletter for today only for the demo and test users.
        var secondSendHourOffset = MathHelpers.Mod(user.SecondSendHour - user.SendHour, 24);
        var currentHourOffset = MathHelpers.Mod(DateHelpers.CurrentHour - user.SendHour, 24);
        var isDemoAndDateIsToday = date == user.TodayOffset && user.Features.HasAnyFlag(Features.Demo | Features.Test);
        if ((oldNewsletters.Count >= (currentHourOffset >= secondSendHourOffset ? 2 : 1)) && !isDemoAndDateIsToday)
        {
            // An old newsletter was found.
            _logger.Log(LogLevel.Information, "Returning old workout for user {Id}", user.Id);
            Logs.AppendLog(user, $"{date}: Returning old workout");
            return await NewsletterOld(user, token, date.Value, oldNewsletters.First());
        }
        // Don't allow backfilling workouts over 1 year ago or in the future.
        else if (date.Value.AddYears(1) < user.TodayOffset || date > user.TodayOffset)
        {
            // A newsletter was not found and the date is not one we want to render a new newsletter for.
            _logger.Log(LogLevel.Information, "Returning no workout for user {Id}", user.Id);
            Logs.AppendLog(user, $"{date}: Returning no workout");
            return null;
        }

        // Context may be null on rest days.
        var context = await BuildWorkoutContext(user, token, date.Value, isSecondWorkoutToday: oldNewsletters.Any() && !isDemoAndDateIsToday);
        if (context == null)
        {
            // See if a previous workout exists, we send that back down so the app doesn't render nothing on rest days.
            var currentWorkout = await _userRepo.GetCurrentWorkout(user, includeVariations: true);
            if (currentWorkout == null)
            {
                _logger.Log(LogLevel.Information, "Returning no workout for user {Id}", user.Id);
                Logs.AppendLog(user, $"{date}: Returning no workout");
                return null;
            }

            _logger.Log(LogLevel.Information, "Returning current workout for user {Id}", user.Id);
            Logs.AppendLog(user, $"{date}: Returning current workout");
            return await NewsletterOld(user, token, currentWorkout.Date, currentWorkout);
        }

        // User is a debug user. They should see the DebugNewsletter instead.
        if (user.Features.HasFlag(Features.Debug))
        {
            _logger.Log(LogLevel.Information, "Returning debug workout for user {Id}", user.Id);
            Logs.AppendLog(user, $"{date}: Returning debug workout");
            return await Debug(context);
        }

        // Current day should be a mobility workout.
        if (context.Frequency == Frequency.Mobility)
        {
            _logger.Log(LogLevel.Information, "Returning off day workout for user {Id}", user.Id);
            Logs.AppendLog(user, $"{date}: Returning off day workout");
            return await OffDayNewsletter(context);
        }

        // Current day should be a strengthening workout.
        _logger.Log(LogLevel.Information, "Returning on day workout for user {Id}", user.Id);
        Logs.AppendLog(user, $"{date}: Returning on day workout");
        return await OnDayNewsletter(context);
    }

    /// <summary>
    /// A newsletter with loads of debug information used for checking data validity.
    /// </summary>
    internal async Task<NewsletterDto?> Debug(WorkoutContext context)
    {
        context.User.Verbosity = Verbosity.Debug;
        var debugExercises = await GetDebugExercises(context.User);
        var newsletter = await CreateAndAddNewsletterToContext(context, exercises: debugExercises);

        await UpdateLastSeenDate(debugExercises);
        return new NewsletterDto
        {
            Verbosity = context.User.Verbosity,
            User = new UserNewsletterDto(context.User.AsType<UserDto>()!, context.Token, context.TimeUntilDeload),
            UserWorkout = newsletter.AsType<UserWorkoutDto>()!,
            MainExercises = debugExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
        };
    }

    /// <summary>
    /// The strength training newsletter.
    /// </summary>
    private async Task<NewsletterDto?> OnDayNewsletter(WorkoutContext context)
    {
        // Choose core before functional. Otherwise, functional movements that target core muscle groups will filter out all core exercises.
        // Choose core before accessory. We want to work variations such as Single Leg Lift from Reverse Plank as a core,
        //  ... but before it gets filtered out by accessory which fills in any gaps in muscles worked.
        var coreExercises = await GetCoreExercises(context);

        // Choose strengthening exercises before warmup and cooldown because of the delayed refresh--we want to continue seeing those exercises in the strengthening sections.
        // Functional movements > sports specific strengthening.
        var functionalExercises = await GetFunctionalExercises(context,
            // Never work the same variation twice.
            excludeVariations: coreExercises);

        // Choose sports before accessory. Sports specific strengthening > general strengthening.
        var sportsExercises = await GetSportsExercises(context,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier.
            excludeExercises: coreExercises.Concat(functionalExercises),
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises),
            // Unset muscles that have already been worked by the core and functional exercises.
            // Disabling to just use muscle target adjustments so that more sports exercises appear.
            _: coreExercises.WorkedMusclesDict(vm => vm.Variation.Strengthens,
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.Strengthens,
                    // Weight secondary muscles as half.
                    addition: coreExercises.WorkedMusclesDict(vm => vm.Variation.Stabilizes, weightDivisor: 2,
                        addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.Stabilizes, weightDivisor: 2)
                    )
                )
            ));

        // Lower the intensity to reduce the risk of injury from heavy-weighted isolation exercises.
        // Choose accessory last. It fills the gaps in any muscle targets.
        var accessoryExercises = await GetAccessoryExercises(context,
            // sa. exclude all Plank variations if we already worked any Plank variation earlier.
            excludeGroups: coreExercises.Concat(functionalExercises).Concat(sportsExercises),
            // sa. exclude all Squat variations if we already worked any Squat variation earlier.
            excludeExercises: coreExercises.Concat(functionalExercises).Concat(sportsExercises),
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises).Concat(sportsExercises),
            // Unset muscles that have already been worked by the core, functional and sports exercises.
            workedMusclesDict: coreExercises.WorkedMusclesDict(vm => vm.Variation.Strengthens,
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.Strengthens,
                    addition: sportsExercises.WorkedMusclesDict(vm => vm.Variation.Strengthens,
                        // Weight secondary muscles as half.
                        addition: coreExercises.WorkedMusclesDict(vm => vm.Variation.Stabilizes, weightDivisor: 2,
                            addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.Stabilizes, weightDivisor: 2,
                                addition: sportsExercises.WorkedMusclesDict(vm => vm.Variation.Stabilizes, weightDivisor: 2)
                            )
                        )
                    )
                )
            ));

        // These are the highest priority, choose these first.
        var rehabExercises = await GetRehabExercises(context,
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises).Concat(sportsExercises).Concat(accessoryExercises));

        // Choose prehab before warmup and cooldown. The user wants extra strengthening for these variations.
        var prehabExercises = await GetPrehabExercises(context,
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises).Concat(sportsExercises).Concat(accessoryExercises).Concat(rehabExercises));

        // Choose warmup before cooldown. We want a better warmup section on strengthening days.
        var warmupExercises = await GetWarmupExercises(context,
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises).Concat(sportsExercises).Concat(accessoryExercises).Concat(rehabExercises).Concat(prehabExercises));

        var cooldownExercises = await GetCooldownExercises(context,
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises).Concat(sportsExercises).Concat(accessoryExercises).Concat(rehabExercises).Concat(prehabExercises).Concat(warmupExercises));

        var newsletter = await CreateAndAddNewsletterToContext(context,
            exercises: coreExercises.Concat(functionalExercises).Concat(sportsExercises).Concat(accessoryExercises).Concat(rehabExercises).Concat(prehabExercises).Concat(warmupExercises).Concat(cooldownExercises).ToList()
        );

        var userViewModel = new UserNewsletterDto(context.User.AsType<UserDto>()!, context.Token, context.TimeUntilDeload);

        if (!context.IsBackfill)
        {
            // Don't update the last seen dates when backfilling workout data so that the user's current workouts are unaffected.
            await UpdateLastSeenDate(exercises: functionalExercises.Concat(sportsExercises).Concat(accessoryExercises).Concat(coreExercises).Concat(rehabExercises).Concat(prehabExercises).Concat(warmupExercises).Concat(cooldownExercises));
        }

        return new NewsletterDto
        {
            User = userViewModel,
            Verbosity = context.User.Verbosity,
            UserWorkout = newsletter.AsType<UserWorkoutDto>()!,
            PrehabExercises = prehabExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            RehabExercises = rehabExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            WarmupExercises = warmupExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            CooldownExercises = cooldownExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            SportsExercises = sportsExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            MainExercises = functionalExercises.Concat(accessoryExercises).Concat(coreExercises).Select(r => r.AsType<ExerciseVariationDto>()!).ToList()
        };
    }

    /// <summary>
    /// The mobility/stretch newsletter for days off strength training.
    /// </summary>
    private async Task<NewsletterDto?> OffDayNewsletter(WorkoutContext context)
    {
        // These are the highest priority, choose these first.
        var rehabExercises = await GetRehabExercises(context);

        // Choose prehab before warmup and cooldown. The user wants extra stretching for these variations.
        var prehabExercises = await GetPrehabExercises(context,
            // Never work the same variation twice.
            excludeVariations: rehabExercises);

        // Reverse the order we choose the core, warmup, and cooldown sections--so we're less likely to get stuck with a multi-purpose variation in just one section.
        // Choose cooldown before warmup. We want a better cooldown section on mobility days.
        var cooldownExercises = await GetCooldownExercises(context,
            // Never work the same variation twice.
            excludeVariations: rehabExercises.Concat(prehabExercises));

        var warmupExercises = await GetWarmupExercises(context,
            // Never work the same variation twice.
            excludeVariations: rehabExercises.Concat(prehabExercises).Concat(cooldownExercises));

        var coreExercises = await GetCoreExercises(context,
            // Never work the same variation twice.
            excludeVariations: rehabExercises.Concat(prehabExercises).Concat(cooldownExercises).Concat(warmupExercises));

        var newsletter = await CreateAndAddNewsletterToContext(context,
            exercises: rehabExercises.Concat(prehabExercises).Concat(cooldownExercises).Concat(warmupExercises).Concat(coreExercises).ToList()
        );

        var userViewModel = new UserNewsletterDto(context.User.AsType<UserDto>()!, context.Token, context.TimeUntilDeload);

        if (!context.IsBackfill)
        {
            // Don't update the last seen dates when backfilling workout data so that the user's current workouts are unaffected.
            await UpdateLastSeenDate(exercises: rehabExercises.Concat(prehabExercises).Concat(cooldownExercises).Concat(warmupExercises).Concat(coreExercises));
        }

        return new NewsletterDto
        {
            User = userViewModel,
            Verbosity = context.User.Verbosity,
            UserWorkout = newsletter.AsType<UserWorkoutDto>()!,
            PrehabExercises = prehabExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            RehabExercises = rehabExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            WarmupExercises = warmupExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            CooldownExercises = cooldownExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList(),
            MainExercises = coreExercises.Select(r => r.AsType<ExerciseVariationDto>()!).ToList()
        };
    }

    /// <summary>
    /// Root route for building out the workout routine newsletter based on a date.
    /// </summary>
    private async Task<NewsletterDto?> NewsletterOld(User user, string token, DateOnly date, UserWorkout newsletter)
    {
        var userViewModel = new UserNewsletterDto(user.AsType<UserDto>()!, token);
        var newsletterViewModel = new NewsletterDto
        {
            Date = date,
            User = userViewModel,
            Verbosity = user.Verbosity,
            UserWorkout = newsletter.AsType<UserWorkoutDto>()!,
        };

        foreach (var rootSection in EnumExtensions.GetMultiValues<Section>().Where(s => s != Section.All))
        {
            var exercises = new List<ExerciseVariationDto>();
            foreach (var section in (Section[])[rootSection, .. EnumExtensions.GetSubValues(rootSection)])
            {
                exercises.AddRange((await new QueryBuilder(section)
                    .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
                    .WithExercises(options =>
                    {
                        options.AddPastVariations(newsletter.UserWorkoutVariations);
                    })
                    .Build()
                    .Query(_serviceScopeFactory))
                    .Select(r => r.AsType<ExerciseVariationDto>()!)
                    // Re-order the exercise variation order to match the original workout.
                    .OrderBy(e => newsletter.UserWorkoutVariations.First(nv => nv.VariationId == e.Variation.Id).Order));
            }

            switch (rootSection)
            {
                case Section.Main:
                    newsletterViewModel.MainExercises = exercises;
                    break;
                case Section.Warmup:
                    newsletterViewModel.WarmupExercises = exercises;
                    break;
                case Section.Cooldown:
                    newsletterViewModel.CooldownExercises = exercises;
                    break;
                case Section.Sports:
                    newsletterViewModel.SportsExercises = exercises;
                    break;
                case Section.Prehab:
                    newsletterViewModel.PrehabExercises = exercises;
                    break;
                case Section.Rehab:
                    newsletterViewModel.RehabExercises = exercises;
                    break;
            }
        }

        return newsletterViewModel;
    }
}
