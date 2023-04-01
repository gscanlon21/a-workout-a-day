using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Web.Code.Extensions;
using Web.Data;
using Web.Data.Query;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;
using Web.Services;
using Web.ViewModels.Newsletter;
using Web.ViewModels.User;

namespace Web.Controllers.User;

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

    private readonly UserService _userService;

    public UserController(CoreContext context, UserService userService) : base(context)
    {
        _userService = userService;
    }

    /// <summary>
    /// Where the user edits their preferences.
    /// </summary>
    [Route("edit")]
    public async Task<IActionResult> Edit(string email, string token, bool? wasUpdated = null)
    {
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeUserExerciseVariations: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var viewModel = new UserEditViewModel(user, token)
        {
            WasUpdated = wasUpdated,
            EquipmentBinder = user.UserEquipments.Select(e => e.EquipmentId).ToArray(),
            IgnoredExerciseBinder = user.UserExercises?.Where(ep => ep.Ignore).Select(e => e.ExerciseId).ToArray(),
            IgnoredVariationBinder = user.UserVariations?.Where(ep => ep.Ignore).Select(e => e.VariationId).ToArray(),
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
            IgnoredVariations = await _context.Variations
                .Where(e => user.UserVariations != null && user.UserVariations.Select(ep => ep.VariationId).Contains(e.Id))
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

        if (ModelState.IsValid)
        {
            try
            {
                viewModel.User = await _userService.GetUser(viewModel.Email, viewModel.Token, includeUserEquipments: true, includeUserExerciseVariations: true);
                if (viewModel.User == null)
                {
                    return NotFound();
                }

                // Ignored Exercises
                var oldUserProgressions = await _context.UserExercises
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredExerciseBinder == null || !viewModel.IgnoredExerciseBinder.Contains(p.ExerciseId))
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

                // Ignored Variations
                var oldUserVariationProgressions = await _context.UserVariations
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredVariationBinder == null || !viewModel.IgnoredVariationBinder.Contains(p.VariationId))
                    .ToListAsync();
                var newUserVariationProgressions = await _context.UserVariations
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredVariationBinder != null && viewModel.IgnoredVariationBinder.Contains(p.VariationId))
                    .ToListAsync();
                foreach (var oldUserVariationProgression in oldUserVariationProgressions)
                {
                    oldUserVariationProgression.Ignore = false;
                }
                foreach (var newUserVariationProgression in newUserVariationProgressions)
                {
                    newUserVariationProgression.Ignore = true;
                }
                _context.Set<UserVariation>().UpdateRange(oldUserVariationProgressions);
                _context.Set<UserVariation>().UpdateRange(newUserVariationProgressions);

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
                viewModel.User.RecoveryMuscle = viewModel.RecoveryMuscle;
                viewModel.User.DeloadAfterEveryXWeeks = viewModel.DeloadAfterEveryXWeeks;
                viewModel.User.RefreshAccessoryEveryXWeeks = viewModel.RefreshAccessoryEveryXWeeks;
                viewModel.User.RefreshFunctionalEveryXWeeks = viewModel.RefreshFunctionalEveryXWeeks;
                viewModel.User.SportsFocus = viewModel.SportsFocus;
                viewModel.User.EmailAtUTCOffset = viewModel.EmailAtUTCOffset;
                viewModel.User.RestDays = viewModel.RestDays;
                viewModel.User.IsNewToFitness = viewModel.IsNewToFitness;
                viewModel.User.IncludeAdjunct = viewModel.IncludeAdjunct;
                viewModel.User.PreferStaticImages = viewModel.PreferStaticImages;
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

            return RedirectToAction(nameof(Edit), new { email, token, WasUpdated = true });
        }

        return await Edit(email, token, wasUpdated: false);
    }

    /// <summary>
    /// Updates the user's LastActive date and redirects them to their final destination.
    /// </summary>
    [Route("redirect", Order = 1)]
    [Route("is-active", Order = 2)]
    public async Task<IActionResult> IAmStillHere(string email, string token, string? redirectTo = null)
    {
        var user = await _userService.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        if (user.Disabled)
        {
            // User is disabled, redirect to the edit page so they can re-enable themselves.
            return RedirectToAction(nameof(UserController.Edit), new { email, token });
        }

        user.LastActive = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.SaveChangesAsync();

        if (redirectTo != null)
        {
            return Redirect(redirectTo);
        }

        return View("StatusMessage", new StatusMessageViewModel($"Thank you."));
    }

    /// <summary>
    /// Reduces the user's progression of an exercise.
    /// </summary>
    [Route("exercise/fallback")]
    public async Task<IActionResult> ThatWorkoutWasTough(string email, int exerciseId, string token)
    {
        var user = await _userService.GetUser(email, token, allowDemoUser: true);
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

        userProgression.Progression = await
            // Stop at the lower bounds of variations
            _context.ExerciseVariations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Min)
            // Stop at the upper bounds of variations
            .Union(_context.ExerciseVariations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Max)
            )
            .Where(mp => mp.HasValue && mp < userProgression.Progression)
            .OrderBy(mp => userProgression.Progression - mp)
            .FirstOrDefaultAsync() ?? UserExercise.MinUserProgression;

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
            Demo = user.IsDemoUser
        });
    }

    /// <summary>
    /// Ignores a variation for a user.
    /// </summary>
    [Route("variation/ignore")]
    public async Task<IActionResult> IgnoreVariation(string email, int exerciseId, int variationId, string token)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var variation = await _context.Variations.FirstOrDefaultAsync(p => p.Id == variationId);
        var exercise = await _context.Exercises.FirstOrDefaultAsync(p => p.Id == exerciseId
            // You shouldn't be able to ignore a recovery or sports track
            && p.SportsFocus == SportsFocus.None && p.RecoveryMuscle == MuscleGroups.None
        );

        // May be null if the exercise was soft/hard deleted
        if (variation == null || exercise == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var exerciseVariations = (await new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
            })
            .WithOrderBy(OrderBy.Progression)
            .WithExercises(x =>
            {
                x.AddExercises(new List<Entities.Exercise.Exercise>(1) { exercise });
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main)
            {
                Verbosity = Models.Newsletter.Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            })
            .ToList();

        await _context.SaveChangesAsync();
        return View(new IgnoreVariationViewModel()
        {
            Variation = variation,
            Exercise = exercise,
            ExerciseVariations = exerciseVariations
        });
    }

    [Route("variation/ignore"), HttpPost]
    public async Task<IActionResult> IgnoreVariationPost(string email, string token, [FromForm] int? exerciseId = null, [FromForm] int? variationId = null)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        if (exerciseId != null)
        {
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
        }

        if (variationId != null)
        {
            var userVariationProgression = await _context.UserVariations
                .Include(p => p.Variation)
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == variationId);

            // May be null if the exercise was soft/hard deleted
            if (userVariationProgression == null)
            {
                return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
            }

            userVariationProgression.Ignore = true;
        }

        await _context.SaveChangesAsync();
        return View("StatusMessage", new StatusMessageViewModel("Your preferences have been saved.")
        {
            AutoCloseInXSeconds = null,
        });
    }

    /// <summary>
    /// Increases the user's progression of an exercise.
    /// </summary>
    [Route("exercise/advance")]
    public async Task<IActionResult> ThatWorkoutWasEasy(string email, int exerciseId, string token)
    {
        var user = await _userService.GetUser(email, token, allowDemoUser: true);
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

        userProgression.Progression = await
            // Stop at the lower bounds of variations
            _context.ExerciseVariations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Min)
            // Stop at the upper bounds of variations
            .Union(_context.ExerciseVariations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Max)
            )
            .Where(mp => mp.HasValue && mp > userProgression.Progression)
            .OrderBy(mp => mp - userProgression.Progression)
            .FirstOrDefaultAsync() ?? UserExercise.MaxUserProgression;

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
            Demo = user.IsDemoUser
        });
    }

    /// <summary>
    /// Shows a form to the user where they can update their Pounds lifted.
    /// </summary>
    [Route("variation/edit")]
    public async Task<IActionResult> EditVariation(string email, int variationId, string token, bool? wasUpdated = null)
    {
        var user = await _userService.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserVariations
            .Include(p => p.Variation)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == variationId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        return View(new UserEditVariationViewModel()
        {
            WasUpdated = wasUpdated,
            Token = token,
            Email = email,
            VariationId = variationId,
            Pounds = userProgression.Pounds,
            VariationName = (await _context.Variations.FirstAsync(v => v.Id == variationId)).Name
        });
    }

    [Route("variation/edit"), HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVariation(string email, string token, UserEditVariationViewModel viewModel)
    {
        if (token != viewModel.Token || email != viewModel.Email)
        {
            return NotFound();
        }

        viewModel.VariationName = (await _context.Variations.FirstAsync(v => v.Id == viewModel.VariationId)).Name;

        if (ModelState.IsValid)
        {
            var user = await _userService.GetUser(viewModel.Email, viewModel.Token);
            if (user == null)
            {
                return NotFound();
            }

            var userProgression = await _context.UserVariations
                .Include(p => p.Variation)
                .FirstAsync(p => p.UserId == user.Id && p.VariationId == viewModel.VariationId);

            userProgression.Pounds = viewModel.Pounds;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserController.EditVariation), new { email, token, viewModel.VariationId, WasUpdated = true });
        }

        return await EditVariation(email, viewModel.VariationId, token, wasUpdated: false);
    }

    /// <summary>
    /// User delete confirmation page.
    /// </summary>
    [Route("delete")]
    public async Task<IActionResult> Delete(string email, string token)
    {
        var user = await _userService.GetUser(email, token);
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
        var user = await _userService.GetUser(email, token);
        if (user != null)
        {
            _context.Newsletters.RemoveRange(await _context.Newsletters.Where(n => n.UserId == user.Id).ToListAsync());
            _context.Users.Remove(user); // Will also remove from ExerciseUserProgressions and EquipmentUsers
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(IndexController.Index), IndexController.Name, new { WasUnsubscribed = true });
    }
}
