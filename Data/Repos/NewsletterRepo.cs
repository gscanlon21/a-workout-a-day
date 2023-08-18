using Core.Code.Extensions;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Code.Extensions;
using Data.Dtos.Newsletter;
using Data.Dtos.User;
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
        var user = await _userRepo.GetUser(email, token, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);

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
        var user = await _userRepo.GetUser(email, token, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
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
        var userViewModel = new UserNewsletterDto(context);
        var viewModel = new NewsletterDto(userViewModel, newsletter)
        {
            MainExercises = debugExercises,
        };

        await UpdateLastSeenDate(debugExercises);

        return viewModel;
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
        // Functional movements > sports specific strengtheing.
        var functionalExercises = await GetFunctionalExercises(context,
            // Never work the same variation twice.
            excludeVariations: coreExercises);

        // Choose sports before accessory. Sports specific strengthening > general strengthening.
        var sportsExercises = await GetSportsExercises(context,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier.
            excludeExercises: coreExercises.Concat(functionalExercises),
            // Never work the same variation twice.
            excludeVariations: coreExercises.Concat(functionalExercises),
            // Unset muscles that have already been worked by the core, functional and sports exercises.
            workedMusclesDict: coreExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                    // Weight secondary muscles as half.
                    addition: coreExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2,
                        addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2)
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
            workedMusclesDict: coreExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                    addition: sportsExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles,
                        // Weight secondary muscles as half.
                        addition: coreExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2,
                            addition: functionalExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2,
                                addition: sportsExercises.WorkedMusclesDict(vm => vm.Variation.SecondaryMuscles, weightDivisor: 2)
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

        var userViewModel = new UserNewsletterDto(context);
        var viewModel = new NewsletterDto(userViewModel, newsletter)
        {
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            CooldownExercises = cooldownExercises,
            SportsExercises = sportsExercises,
            MainExercises = functionalExercises.Concat(accessoryExercises).Concat(coreExercises).ToList()
        };

        // Functional exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: functionalExercises,
            refreshAfter: StartOfWeek.AddDays(7 * context.User.RefreshFunctionalEveryXWeeks));
        // Accessory exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: sportsExercises.Concat(accessoryExercises),
            refreshAfter: StartOfWeek.AddDays(7 * context.User.RefreshAccessoryEveryXWeeks));
        // Other exercises. Refresh every day.
        await UpdateLastSeenDate(exercises: coreExercises.Concat(rehabExercises).Concat(prehabExercises).Concat(warmupExercises).Concat(cooldownExercises));

        return viewModel;
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

        var userViewModel = new UserNewsletterDto(context);
        var viewModel = new NewsletterDto(userViewModel, newsletter)
        {
            PrehabExercises = prehabExercises,
            RehabExercises = rehabExercises,
            WarmupExercises = warmupExercises,
            CooldownExercises = cooldownExercises,
            MainExercises = coreExercises
        };

        // Other exercises. Refresh every day.
        await UpdateLastSeenDate(exercises: rehabExercises.Concat(prehabExercises).Concat(cooldownExercises).Concat(warmupExercises).Concat(coreExercises));

        return viewModel;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter based on a date.
    /// </summary>
    private async Task<NewsletterDto?> NewsletterOld(User user, string token, DateOnly date, UserWorkout newsletter)
    {
        var userViewModel = new UserNewsletterDto(user, token);
        var newsletterViewModel = new NewsletterDto(userViewModel, newsletter)
        {
            Today = date,
        };

        foreach (var rootSection in EnumExtensions.GetMultiValues32<Section>())
        {
            var exercises = new List<ExerciseDto>();
            foreach (var section in EnumExtensions.GetSubValues32(rootSection))
            {
                exercises.AddRange((await new QueryBuilder(section)
                    .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
                    .WithExercises(options =>
                    {
                        options.AddPastExerciseVariations(newsletter.UserWorkoutExerciseVariations);
                    })
                    .Build()
                    .Query(_context))
                    .Select(r => new ExerciseDto(r, newsletter.Intensity, newsletter.IsDeloadWeek))
                    .OrderBy(e => newsletter.UserWorkoutExerciseVariations.First(nv => nv.ExerciseVariationId == e.ExerciseVariation.Id).Order)
                    .ToList());
            }

            switch (rootSection)
            {
                case Section.Cooldown:
                    newsletterViewModel.CooldownExercises = exercises;
                    break;
                case Section.Warmup:
                    newsletterViewModel.WarmupExercises = exercises;
                    break;
                case Section.Main:
                    newsletterViewModel.MainExercises = exercises;
                    break;
                case Section.Sports:
                    newsletterViewModel.SportsExercises = exercises;
                    break;
                case Section.Rehab:
                    newsletterViewModel.RehabExercises = exercises;
                    break;
                case Section.Prehab:
                    newsletterViewModel.PrehabExercises = exercises;
                    break;
            }
        }

        return newsletterViewModel;
    }
}
