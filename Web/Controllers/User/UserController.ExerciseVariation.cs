using Core.Consts;
using Core.Models.Newsletter;
using Data.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Web.Views.Shared.Components.ManageVariation;
using Web.Views.User;

namespace Web.Controllers.User;

public partial class UserController
{
    /// <summary>
    /// Shows a form to the user where they can update their Pounds lifted.
    /// </summary>
    [HttpGet, Route("{section:section}/{exerciseId}/{variationId}")]
    public async Task<IActionResult> ManageExerciseVariation(string email, string token, int exerciseId, int variationId, Section section, bool? wasUpdated = null)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var hasVariation = await _context.UserVariations
            // Variations are managed per section, so ignoring variations
            // ... for None sections that are only for managing exercises.
            .Where(uv => uv.Section == section && section != Section.None)
            .AnyAsync(uv => uv.UserId == user.Id && uv.VariationId == variationId);

        return View(new ManageExerciseVariationViewModel()
        {
            User = user,
            WasUpdated = wasUpdated,
            HasVariation = hasVariation,
            Parameters = new(section, email, token, exerciseId, variationId),
        });
    }

    /// <summary>
    /// Reduces the user's progression of an exercise.
    /// </summary>
    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/r", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/regress", Order = 2)]
    public async Task<IActionResult> RegressExercise(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // May be null if the exercise was soft/hard deleted
        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);
        if (userExercise == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userExercise.Progression = await _context.ExercisePrerequisites
            // Stop at the postrequisite proficiency levels.
            .Where(ep => ep.PrerequisiteExerciseId == exerciseId).Select(ep => (int?)ep.Proficiency)
            // Stop at the lower bounds of variations.
            .Union(_context.Variations.Where(ev => ev.ExerciseId == exerciseId).Select(ev => ev.Progression.Min))
            // Stop at the upper bounds of variations.
            .Union(_context.Variations.Where(ev => ev.ExerciseId == exerciseId).Select(ev => ev.Progression.Max))
            .Where(mp => mp.HasValue && mp < userExercise.Progression)
            .OrderBy(mp => userExercise.Progression - mp)
            .FirstOrDefaultAsync() ?? UserConsts.MinUserProgression;

        var validationContext = new ValidationContext(userExercise) { MemberName = nameof(userExercise.Progression) };
        if (Validator.TryValidateProperty(userExercise.Progression, validationContext, null))
        {
            await _context.SaveChangesAsync();
        };

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    /// <summary>
    /// Increases the user's progression of an exercise.
    /// </summary>
    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/p", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/progress", Order = 2)]
    public async Task<IActionResult> ProgressExercise(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // May be null if the exercise was soft/hard deleted
        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);
        if (userExercise == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userExercise.Progression = await _context.ExercisePrerequisites
            // Stop at the postrequisite proficiency levels.
            .Where(ep => ep.PrerequisiteExerciseId == exerciseId).Select(ep => (int?)ep.Proficiency)
            // Stop at the lower bounds of variations.
            .Union(_context.Variations.Where(ev => ev.ExerciseId == exerciseId).Select(ev => ev.Progression.Min))
            // Stop at the upper bounds of variations.
            .Union(_context.Variations.Where(ev => ev.ExerciseId == exerciseId).Select(ev => ev.Progression.Max))
            .Where(mp => mp.HasValue && mp > userExercise.Progression)
            .OrderBy(mp => mp - userExercise.Progression)
            .FirstOrDefaultAsync() ?? UserConsts.MaxUserProgression;

        var validationContext = new ValidationContext(userExercise) { MemberName = nameof(userExercise.Progression) };
        if (Validator.TryValidateProperty(userExercise.Progression, validationContext, null))
        {
            await _context.SaveChangesAsync();
        };

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/ie", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/ignore-exercise", Order = 2)]
    public async Task<IActionResult> IgnoreExercise(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.Exercise.Variations.Any(v => v.Id == variationId));

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Ignore = !userProgression.Ignore;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/rv", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/refresh-variation", Order = 2)]
    public async Task<IActionResult> RefreshVariation(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserVariations
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.VariationId == variationId && ue.Section == section);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.RefreshAfter = null;
        userProgression.LastSeen = userProgression.LastSeen > DateHelpers.Today ? DateHelpers.Today : userProgression.LastSeen;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/iv", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/ignore-variation", Order = 2)]
    public async Task<IActionResult> IgnoreVariation(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariationProgression = await _context.UserVariations
            .Where(uv => uv.UserId == user.Id)
            .FirstOrDefaultAsync(uv => uv.VariationId == variationId && uv.Section == section);

        // May be null if the exercise was soft/hard deleted
        if (userVariationProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariationProgression.Ignore = !userVariationProgression.Ignore;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost, Route("{section:section}/{exerciseId}/{variationId}/log")]
    public async Task<IActionResult> LogVariation(string email, string token, int exerciseId, int variationId, Section section, ManageVariationViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = false });
        }

        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return NotFound();
        }

        // Set the new weight on the UserVariation.
        var userVariation = await _context.UserVariations
            .Include(p => p.Variation)
            .FirstAsync(p => p.UserId == user.Id && p.VariationId == variationId && p.Section == section);

        // Apply refresh padding immediately.
        if (viewModel.PadRefreshXWeeks != userVariation.PadRefreshXWeeks && userVariation.LastSeen > DateOnly.MinValue)
        {
            var difference = viewModel.PadRefreshXWeeks - userVariation.PadRefreshXWeeks; // 11 new - 1 old = 10 weeks.
            userVariation.LastSeen = userVariation.LastSeen.AddDays(7 * difference); // Add 70 days onto the LastSeen date.
        }

        // Apply refresh lagging immediately.
        if (viewModel.LagRefreshXWeeks != userVariation.LagRefreshXWeeks)
        {
            var difference = viewModel.LagRefreshXWeeks - userVariation.LagRefreshXWeeks; // 11 new - 1 old = 10 weeks.
            var refreshAfterOrTodayWithLag = (userVariation.RefreshAfter ?? DateHelpers.Today).AddDays(7 * difference);
            userVariation.RefreshAfter = refreshAfterOrTodayWithLag > DateHelpers.Today ? refreshAfterOrTodayWithLag : null;
            // NOTE: Not updating the LastSeen date if RefreshAfter is null, so the user may see this variation again tomorrow.
        }

        userVariation.Weight = viewModel.Weight;
        // Don't let the demo user alter their Sets/Reps/Secs
        // ... since those are used in the training volume calculations.
        userVariation.Sets = user.IsDemoUser ? default : viewModel.Sets;
        userVariation.Reps = user.IsDemoUser ? default : viewModel.Reps;
        userVariation.Secs = user.IsDemoUser ? default : viewModel.Secs;
        userVariation.Notes = user.IsDemoUser ? default : viewModel.Notes;
        userVariation.LagRefreshXWeeks = user.IsDemoUser ? default : viewModel.LagRefreshXWeeks;
        userVariation.PadRefreshXWeeks = user.IsDemoUser ? default : viewModel.PadRefreshXWeeks;

        // Log the weight as a UserWeight.
        var todaysUserWeight = await _context.UserVariationLogs
            .Where(uw => uw.UserVariationId == userVariation.Id)
            .FirstOrDefaultAsync(uw => uw.Date == DateHelpers.Today);
        if (todaysUserWeight != null)
        {
            todaysUserWeight.Weight = userVariation.Weight;
            todaysUserWeight.Sets = user.IsDemoUser ? default : userVariation.Sets;
            todaysUserWeight.Reps = user.IsDemoUser ? default : userVariation.Reps;
            todaysUserWeight.Secs = user.IsDemoUser ? default : userVariation.Secs;
        }
        else
        {
            _context.Add(new UserVariationLog()
            {
                Date = DateHelpers.Today,
                Weight = userVariation.Weight,
                UserVariationId = userVariation.Id,
                Sets = user.IsDemoUser ? default : userVariation.Sets,
                Reps = user.IsDemoUser ? default : userVariation.Reps,
                Secs = user.IsDemoUser ? default : userVariation.Secs,
            });
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }
}
