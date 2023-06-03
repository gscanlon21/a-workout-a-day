using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.ViewData;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Newsletter;

public partial class NewsletterController
{
    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetDebugExercises(Entities.User.User user, string token, int count = 1)
    {
        var baseQuery = _context.ExerciseVariations
            .AsNoTracking()
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
            });

        return (await baseQuery.ToListAsync())
            .GroupBy(i => new { i.Exercise.Id, LastSeen = i.UserExercise?.LastSeen ?? DateOnly.MinValue })
            .OrderBy(a => a.Key.LastSeen)
                .ThenBy(a => Guid.NewGuid())
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
    [Route("debug")]
    public async Task<IActionResult> Debug(string email, string token)
    {
        // The debug user is disabled, not checking that or rest days.
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeExerciseVariations: true);
        if (user == null || user.Disabled || user.RestDays.HasFlag(DaysExtensions.FromDate(Today))
            // User is not a debug user. They should see the Newsletter instead.
            || !user.Features.HasFlag(Features.Debug))
        {
            return NoContent();
        }

        // User was already sent a newsletter today.
        // Checking for variations because we create a dummy newsletter record to advance the workout split.
        if (await _context.Newsletters.AnyAsync(n => n.UserId == user.Id && n.NewsletterVariations.Any() && n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Features.HasFlag(Features.ManyEmails))
        {
            return NoContent();
        }

        user.EmailVerbosity = Verbosity.Debug;
        await AddMissingUserExerciseVariationRecords(user);
        var todaysNewsletterRotation = await _userService.GetTodaysNewsletterRotation(user);

        var debugExercises = await GetDebugExercises(user, token, count: 1);

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, user.Frequency, needsDeload: false,
            variations: debugExercises
        );
        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var viewModel = new DebugViewModel(user, token)
        {
            AllEquipment = equipmentViewModel,
            DebugExercises = debugExercises,
        };

        await UpdateLastSeenDate(debugExercises);

        ViewData[ViewData_Newsletter.NeedsDeload] = false;
        return View(nameof(Debug), viewModel);
    }
}
