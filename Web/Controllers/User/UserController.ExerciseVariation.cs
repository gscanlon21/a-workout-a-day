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
    [HttpGet]
    [Route("{section:section}/{exerciseId}/{variationId}", Order = 1)]
    public async Task<IActionResult> ManageExerciseVariation(string email, string token, int exerciseId, int variationId, Section section, bool? wasUpdated = null)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var parameters = new ManageExerciseVariationDto.Params(section, email, token, exerciseId, variationId);
        var hasVariation = await _context.UserVariations
            // Variations are managed per section, so ignoring variations for .None sections that are only for managing exercises.
            .Where(uv => uv.Section == section && section != Section.None)
            .AnyAsync(uv => uv.UserId == user.Id && uv.VariationId == variationId);

        return View(new ManageExerciseVariationDto()
        {
            Parameters = parameters,
            WasUpdated = wasUpdated,
            HasVariation = hasVariation,
            User = user,
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

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/l", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/log", Order = 2)]
    public async Task<IActionResult> LogVariation(string email, string token, int exerciseId, int variationId, Section section, ManageVariationViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
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
            if (viewModel.PadRefreshXWeeks != userVariation.PadRefreshXWeeks)
            {
                var difference = viewModel.PadRefreshXWeeks - userVariation.PadRefreshXWeeks; // 11 new - 1 old = 10 weeks.
                userVariation.LastSeen = userVariation.LastSeen.AddDays(7 * difference); // Add 70 days onto the LastSeen date.
            }

            userVariation.Sets = viewModel.Sets;
            userVariation.Reps = viewModel.Reps;
            userVariation.Secs = viewModel.Secs;
            userVariation.Notes = viewModel.Notes;
            userVariation.Weight = viewModel.Weight;
            userVariation.LagRefreshXWeeks = viewModel.LagRefreshXWeeks;
            userVariation.PadRefreshXWeeks = viewModel.PadRefreshXWeeks;

            // Log the weight as a UserWeight.
            var todaysUserWeight = await _context.UserVariationLogs
                .Where(uw => uw.UserVariationId == userVariation.Id)
                .FirstOrDefaultAsync(uw => uw.Date == DateHelpers.Today);
            if (todaysUserWeight != null)
            {
                todaysUserWeight.Weight = userVariation.Weight;
                todaysUserWeight.Sets = userVariation.Sets;
                todaysUserWeight.Reps = userVariation.Reps;
                todaysUserWeight.Secs = userVariation.Secs;
            }
            else
            {
                _context.Add(new UserVariationLog()
                {
                    Date = DateHelpers.Today,
                    UserVariationId = userVariation.Id,
                    Weight = userVariation.Weight,
                    Sets = userVariation.Sets,
                    Reps = userVariation.Reps,
                    Secs = userVariation.Secs,
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
        }

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = false });
    }
}
