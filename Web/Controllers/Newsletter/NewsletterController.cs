using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.Extensions;
using Web.Code.ViewData;
using Web.Data;
using Web.Entities.User;
using Web.Models.User;
using Web.Services;
using Web.ViewModels.Newsletter;
using Web.ViewModels.User;

namespace Web.Controllers.Newsletter;

[Route("newsletter")]
public partial class NewsletterController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Newsletter";

    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly UserService _userService;

    public NewsletterController(CoreContext context, UserService userService, IServiceScopeFactory serviceScopeFactory) : base(context)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
    }

    /// <summary>
    /// Root route for building out the the workout routine newsletter.
    /// </summary>
    [Route("demo", Order = 1)]
    [Route("{email}", Order = 2)]
    public async Task<IActionResult> Newsletter(string email = "demo@test.finerfettle.com", string token = "00000000-0000-0000-0000-000000000000")
    {
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeVariations: true, allowDemoUser: true);
        if (user == null || user.Disabled || user.RestDays.HasFlag(RestDaysExtensions.FromDate(Today))
            // User is a debug user. They should see the DebugNewsletter instead.
            || user.Features.HasFlag(Features.Debug))
        {
            return NoContent();
        }

        // User was already sent a newsletter today
        if (await _context.Newsletters.Where(n => n.UserId == user.Id).AnyAsync(n => n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails))
        {
            return NoContent();
        }

        // User has received an email with a confirmation message, but they did not click to confirm their account
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id) && user.LastActive == null)
        {
            return NoContent();
        }

        // The exercise query runner requires UserExercise/UserExerciseVariation/UserVariation records to have already been made.
        // There is a small chance for a race-condition if Exercise/ExerciseVariation/Variation data is updated after these run in.
        // I am not concerned about that possiblity because the data changes infrequently and the newsletter will with the next trigger.
        _context.AddMissing(await _context.UserExercises.Where(ue => ue.UserId == user.Id).Select(ue => ue.ExerciseId).ToListAsync(),
            await _context.Exercises.Select(e => new { e.Id, e.Proficiency }).ToListAsync(), k => k.Id, e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserExercise.MinUserProgression : e.Proficiency });

        _context.AddMissing(await _context.UserExerciseVariations.Where(ue => ue.UserId == user.Id).Select(uev => uev.ExerciseVariationId).ToListAsync(),
            await _context.ExerciseVariations.Select(ev => ev.Id).ToListAsync(), evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id });

        _context.AddMissing(await _context.UserVariations.Where(ue => ue.UserId == user.Id).Select(uv => uv.VariationId).ToListAsync(),
            await _context.Variations.Select(v => v.Id).ToListAsync(), vId => new UserVariation() { VariationId = vId, UserId = user.Id });

        await _context.SaveChangesAsync();

        (var needsDeload, var timeUntilDeload) = await _userService.CheckNewsletterDeloadStatus(user);
        var todaysNewsletterRotation = await GetTodaysNewsletterRotation(user);
        var todaysMainIntensityLevel = user.StrengtheningPreference.ToIntensityLevel(needsDeload);

        // Choose cooldown first
        var cooldownExercises = await GetCooldownExercises(user, todaysNewsletterRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysNewsletterRotation, token,
            // sa. exclude the same Cat/Cow variation we worked as a cooldown
            excludeVariations: cooldownExercises);

        var functionalExercises = await GetFunctionalExercises(user, token, needsDeload, todaysMainIntensityLevel, todaysNewsletterRotation,
            // sa. exclude the same Pushup Plus variation we worked for a warmup
            excludeVariations: warmupExercises.Select(e => e.Variation));

        (var accessoryExercises, var extraExercises) = await GetAccessoryExtraExercises(user, token, needsDeload, todaysMainIntensityLevel, todaysNewsletterRotation,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            excludeGroups: functionalExercises,
            // sa. exclude all Squat variations if we already worked any Squat variation earlier
            excludeExercises: functionalExercises,
            // sa. exclude the same Deadbug variation we worked for a warmup
            // sa. exclude the same Bar Hang variation we worked for a warmup
            excludeVariations: functionalExercises.Concat(warmupExercises).Concat(cooldownExercises),
            // Unset muscles that have already been worked by the functional exercises
            workedMusclesDict: functionalExercises.WorkedMusclesDict(vm => vm.Variation.StrengthMuscles));

        var recoveryExercises = await GetRecoveryExercises(user, token);
        var sportsExercises = await GetSportsExercises(user, token, todaysNewsletterRotation, todaysMainIntensityLevel, needsDeload);

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload: needsDeload,
            strengthExercises: functionalExercises.Concat(accessoryExercises).Concat(extraExercises)
        );
        var userViewModel = new UserNewsletterViewModel(user, token)
        {
            TimeUntilDeload = timeUntilDeload,
        };
        var viewModel = new NewsletterViewModel(userViewModel, newsletter)
        {
            RecoveryExercises = recoveryExercises,
            WarmupExercises = warmupExercises,
            MainExercises = functionalExercises.Concat(accessoryExercises).ToList(),
            ExtraExercises = extraExercises,
            SportsExercises = sportsExercises,
            CooldownExercises = cooldownExercises
        };

        // Functional exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: functionalExercises, noLog: Enumerable.Empty<ExerciseViewModel>(), refreshAfter: StartOfWeek.AddDays(7 * user.RefreshFunctionalEveryXWeeks));
        // Accessory exercises. Refresh at the start of the week.
        await UpdateLastSeenDate(exercises: accessoryExercises, noLog: extraExercises, refreshAfter: StartOfWeek.AddDays(7 * user.RefreshAccessoryEveryXWeeks));
        // Other exercises. Refresh every day.
        await UpdateLastSeenDate(exercises: warmupExercises.Concat(cooldownExercises).Concat(recoveryExercises ?? new List<ExerciseViewModel>()).Concat(sportsExercises ?? new List<ExerciseViewModel>()), 
            noLog: Enumerable.Empty<ExerciseViewModel>());

        ViewData[ViewData_Newsletter.NeedsDeload] = needsDeload;
        return View(nameof(Newsletter), viewModel);
    }
}
