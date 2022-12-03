using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Extensions;
using Web.ViewModels.User;
using System.ComponentModel.DataAnnotations;
using Web.Entities.User;
using Web.Models.Exercise;
using System.Numerics;
using Web.Models.User;

namespace Web.Controllers;

[Route("user/{email}")]
public class UserController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "User";

    /// <summary>
    /// The reason for disabling the user's account when directed by the user.
    /// </summary>
    public const string UserDisabledByUserReason = "User disabled";

    /// <summary>
    /// Message to show to the user when a link has expired.
    /// </summary>
    public const string LinkExpiredMessage = "This link has expired.";

    public UserController(CoreContext context) : base(context) { }

    #region Helpers

    /// <summary>
    /// Grab a user from the db with a specific token
    /// </summary>
    private async Task<User?> GetUser(string email, string token, bool includeUserEquipments = false, bool includeUserExercises = false, bool allowDemoUser = false)
    {
        if (!allowDemoUser && email == Entities.User.User.DemoUser)
        {
            throw new ArgumentException("User not authorized.", nameof(email));
        }

        IQueryable<User> query = _context.Users;

        if (includeUserEquipments)
        {
            query = query.Include(u => u.UserEquipments);
        }

        if (includeUserExercises)
        {
            query = query.Include(u => u.UserExercises);
        }

        return await query.FirstOrDefaultAsync(u => u.Email == email && (u.UserTokens.Any(ut => ut.Token == token) || email == Entities.User.User.DemoUser));
    }

    #endregion

    [Route("edit")]
    public async Task<IActionResult> Edit(string email, string token)
    {
        if (_context.Users == null)
        {
            return NotFound();
        }

        var user = await GetUser(email, token, includeUserEquipments: true, includeUserExercises: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var viewModel = new UserEditViewModel(user, token)
        {
            EquipmentBinder = user.UserEquipments.Select(e => e.EquipmentId).ToArray(),
            IgnoredExerciseBinder = user.UserExercises?.Where(ep => ep.Ignore).Select(e => e.ExerciseId).ToArray(),
            Equipment = await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync(),
            IgnoredExercises = await _context.Exercises
                .Where(e => e.RecoveryMuscle == MuscleGroups.None) // Don't let the user ignore recovery tracks
                .Where(e => e.SportsFocus == SportsFocus.None) // Don't let the user ignore sports tracks
                .Where(e => user.UserExercises != null && user.UserExercises.Select(ep => ep.ExerciseId).Contains(e.Id))
                .OrderBy(e => e.Name)
                .ToListAsync(),
        };

        return View(viewModel);
    }

    [Route("edit"), HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string email, string token, UserEditViewModel viewModel)
    {
        if (token != viewModel.Token || email != viewModel.Email)
        {
            return NotFound();
        }

        viewModel.Equipment = await _context.Equipment.Where(e => e.DisabledReason == null).OrderBy(e => e.Name).ToListAsync();

        if (ModelState.IsValid)
        {
            try
            {
                viewModel.User = await GetUser(viewModel.Email, viewModel.Token, includeUserEquipments: true, includeUserExercises: true);
                if (viewModel.User == null)
                {
                    return NotFound();
                }

                var oldUserProgressions = await _context.UserExercises
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredExerciseBinder != null && !viewModel.IgnoredExerciseBinder.Contains(p.ExerciseId))
                    .ToListAsync();
                var newUserProgressions = await _context.UserExercises
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredExerciseBinder != null && viewModel.IgnoredExerciseBinder.Contains(p.ExerciseId))
                    .ToListAsync();
                foreach (var oldUserProgression in oldUserProgressions)
                {
                    oldUserProgression.Ignore = false;
                }
                foreach (var newUserProgression in newUserProgressions)
                {
                    newUserProgression.Ignore = true;
                }
                _context.Set<UserExercise>().UpdateRange(oldUserProgressions);
                _context.Set<UserExercise>().UpdateRange(newUserProgressions);

                if (viewModel.RecoveryMuscle != MuscleGroups.None)
                {
                    // If any exercise's variation's muscle is worked by the recovery muscle, lower it's progression level
                    var progressions = _context.UserExercises
                        .Where(up => up.UserId == viewModel.User.Id)
                        .Where(up => up.Exercise.ExerciseVariations.Select(ev => ev.Variation).Any(v => v.StrengthMuscles.HasFlag(viewModel.RecoveryMuscle)));
                    foreach (var progression in progressions)
                    {
                        progression.Progression = UserExercise.MinUserProgression;
                    }
                    _context.Set<UserExercise>().UpdateRange(progressions);
                }

                var newEquipment = await _context.Equipment.Where(e =>
                    viewModel.EquipmentBinder != null && viewModel.EquipmentBinder.Contains(e.Id)
                ).ToListAsync();
                _context.TryUpdateManyToMany(viewModel.User.UserEquipments, newEquipment.Select(e =>
                    new UserEquipment() 
                    {
                        EquipmentId = e.Id,
                        UserId = viewModel.User.Id
                    }), 
                    x => x.EquipmentId
                );

                viewModel.User.EmailVerbosity = viewModel.EmailVerbosity;
                viewModel.User.PrefersWeights = viewModel.PrefersWeights;
                viewModel.User.RecoveryMuscle = viewModel.RecoveryMuscle;
                viewModel.User.DeloadAfterEveryXWeeks = viewModel.DeloadAfterEveryXWeeks;
                viewModel.User.SportsFocus = viewModel.SportsFocus;
                viewModel.User.EmailAtUTCOffset = viewModel.EmailAtUTCOffset;
                viewModel.User.RestDays = viewModel.RestDays;
                viewModel.User.IncludeBonus = viewModel.IncludeBonus;
                viewModel.User.IsNewToFitness = viewModel.IsNewToFitness;
                viewModel.User.IncludeAdjunct = viewModel.IncludeAdjunct;
                viewModel.User.StrengtheningPreference = viewModel.StrengtheningPreference;
                viewModel.User.Frequency = viewModel.Frequency;

                if (viewModel.User.Disabled != viewModel.Disabled)
                {
                    viewModel.User.DisabledReason = viewModel.Disabled ? UserDisabledByUserReason : null;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(_context.Users?.Any(e => e.Email == viewModel.Email)).GetValueOrDefault())
                {
                    // User does not exist
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            viewModel.WasUpdated = true;
            return View(viewModel);
        }

        viewModel.WasUpdated = false;
        return View(viewModel);
    }

    [Route("redirect", Order = 1)]
    [Route("is-active", Order = 2)]
    public async Task<IActionResult> IAmStillHere(string email, string token, string? redirectTo = null)
    {
        if (_context.Users == null)
        {
            return NotFound();
        }

        var user = await GetUser(email, token, allowDemoUser: true);
        if (user != null)
        {
            if (user.Disabled)
            {
                // User is disabled, redirect to the edit page so they can re-enable themselves.
                return RedirectToAction(nameof(UserController.Edit), new { email, token });
            }

            user.LastActive = DateOnly.FromDateTime(DateTime.UtcNow);
            await _context.SaveChangesAsync();
        }

        if (redirectTo != null && (!IsInternalDomain(new Uri(redirectTo)) || user != null))
        {
            return Redirect(redirectTo);
        }

        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        return View("StatusMessage", new StatusMessageViewModel($"Thank you."));
    }

    [Route("exercise/fallback")]
    public async Task<IActionResult> ThatWorkoutWasTough(string email, int exerciseId, string token)
    {
        if (_context.Users == null)
        {
            return NotFound();
        }

        var user = await GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Progression = (await _context.ExerciseVariations
            .Where(ev => ev.ExerciseId == exerciseId)
            .Select(ev => ev.Progression.Min)
            .Where(mp => mp.HasValue && mp < userProgression.Progression)
            .Union(_context.ExerciseVariations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Max)
                .Where(mp => mp.HasValue && mp < userProgression.Progression)
            )
            .OrderBy(mp => userProgression.Progression - mp)
            .FirstOrDefaultAsync()) ?? UserExercise.MinUserProgression;

        var validationContext = new ValidationContext(userProgression)
        {
            MemberName = nameof(userProgression.Progression)
        };
        if (Validator.TryValidateProperty(userProgression.Progression, validationContext, null))
        {
            await _context.SaveChangesAsync();
        };

        return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. Your new progression level for {userProgression.Exercise.Name} is {userProgression.Progression}%.")
        {
            Demo = user.Email == Entities.User.User.DemoUser
        });
    }

    [Route("exercise/ignore")]
    public async Task<IActionResult> IgnoreExercise(string email, int exerciseId, string token)
    {
        if (_context.Users == null)
        {
            return NotFound();
        }

        var user = await GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // You can't ignore recovery or sports tracks
        if (userProgression.Exercise.IsPlainExercise)
        {
            userProgression.Ignore = true;
        }

        await _context.SaveChangesAsync();
        return View("StatusMessage", new StatusMessageViewModel("Your preferences have been saved."));
    }

    [Route("exercise/advance")]
    public async Task<IActionResult> ThatWorkoutWasEasy(string email, int exerciseId, string token)
    {
        if (_context.Users == null)
        {
            return NotFound();
        }

        var user = await GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Progression = (await _context.ExerciseVariations
            .Where(ev => ev.ExerciseId == exerciseId)
            .Select(ev => ev.Progression.Min)
            .Where(mp => mp.HasValue && mp > userProgression.Progression)
            .Union(_context.ExerciseVariations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Max)
                .Where(mp => mp.HasValue && mp > userProgression.Progression)
            )
            .OrderBy(mp => mp - userProgression.Progression)
            .FirstOrDefaultAsync()) ?? UserExercise.MaxUserProgression;

        var validationContext = new ValidationContext(userProgression)
        {
            MemberName = nameof(userProgression.Progression)
        };
        if (Validator.TryValidateProperty(userProgression.Progression, validationContext, null))
        {
            await _context.SaveChangesAsync();
        };

        return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. Your new progression level for {userProgression.Exercise.Name} is {userProgression.Progression}%.")
        {
            Demo = user.Email == Entities.User.User.DemoUser
        });
    }

    [Route("delete")]
    public async Task<IActionResult> Delete(string email, string token)
    {
        if (_context.Users == null)
        {
            return NotFound();
        }

        var user = await GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        return View(new UserEditViewModel(user, token));
    }

    [Route("delete"), HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string email, string token)
    {
        if (_context.Users == null)
        {
            return Problem("Entity set 'CoreContext.Users' is null.");
        }

        var user = await GetUser(email, token);
        if (user != null)
        {
            _context.Newsletters.RemoveRange(await _context.Newsletters.Where(n => n.User == user).ToListAsync());
            _context.Users.Remove(user); // Will also remove from ExerciseUserProgressions and EquipmentUsers
        }
        
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(IndexController.Index), IndexController.Name, new { WasUnsubscribed = true });
    }
}
