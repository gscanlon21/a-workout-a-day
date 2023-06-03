using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Web.Code.Extensions;
using Web.Code.TempData;
using Web.Data;
using Web.Data.Query;
using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Models.Exercise;
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
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeUserExerciseVariations: true, includeMuscles: true, includeFrequencies: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userFrequencies = (await _userService.GetCurrentAndUpcomingRotations(user)).OrderBy(f => f.Id).Select(f => new UserEditFrequencyViewModel(f)).ToList();

        while (userFrequencies.Count < UserFrequency.MaxPerUser)
        {
            userFrequencies.Add(new UserEditFrequencyViewModel() { Day = userFrequencies.Count + 1 });
        }

        var viewModel = new UserEditViewModel(user, token)
        {
            WasUpdated = wasUpdated,
            UserFrequencies = userFrequencies,
            EquipmentBinder = user.UserEquipments.Select(e => e.EquipmentId).ToArray(),
            IgnoredExerciseBinder = user.UserExercises?.Where(ep => ep.Ignore).Select(e => e.ExerciseId).ToArray(),
            IgnoredVariationBinder = user.UserVariations?.Where(ep => ep.Ignore).Select(e => e.VariationId).ToArray(),
            Equipment = await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync(),
            IgnoredExercises = await _context.Exercises
                .Where(e => user.UserExercises != null && user.UserExercises.Select(ep => ep.ExerciseId).Contains(e.Id))
                .OrderBy(e => e.Name)
                .ToListAsync(),
            IgnoredVariations = await _context.Variations
                .Where(e => user.UserVariations != null && user.UserVariations.Select(ep => ep.VariationId).Contains(e.Id))
                .OrderBy(e => e.Name)
                .ToListAsync(),
        };

        viewModel.TheIgnoredExercises = (await new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithOrderBy(OrderBy.Progression)
            .WithExercises(x =>
            {
                x.AddExercises(viewModel.IgnoredExercises.Where(e => viewModel.IgnoredExerciseBinder != null && viewModel.IgnoredExerciseBinder.Contains(e.Id)));
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main)
            {
                Verbosity = Models.Newsletter.Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            })
            .ToList();

        viewModel.TheIgnoredVariations = (await new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithOrderBy(OrderBy.Progression)
            .WithExercises(x =>
            {
                x.AddVariations(viewModel.IgnoredVariations.Where(v => viewModel.IgnoredVariationBinder != null && viewModel.IgnoredVariationBinder.Contains(v.Id)));
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main)
            {
                Verbosity = Models.Newsletter.Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            }).ToList();

        return View("Edit", viewModel);
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
                viewModel.User = await _userService.GetUser(viewModel.Email, viewModel.Token, includeUserEquipments: true, includeFrequencies: true, includeUserExerciseVariations: true);
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

                var rehabMuscleGroup = viewModel.RehabFocus.As<MuscleGroups>();
                if (rehabMuscleGroup != MuscleGroups.None && viewModel.User.RehabFocus != viewModel.RehabFocus)
                {
                    // If any exercise's variation's muscle is worked by the (new) recovery muscle, lower it's progression level and un-ignore it.
                    var progressions = _context.UserExercises
                        .Where(up => up.UserId == viewModel.User.Id)
                        .Where(up => up.Exercise.ExerciseVariations.Select(ev => ev.Variation).Any(v => v.StrengthMuscles.HasFlag(rehabMuscleGroup)));
                    foreach (var progression in progressions)
                    {
                        progression.Ignore = false;
                        progression.Progression = UserExercise.MinUserProgression;
                    }
                    _context.Set<UserExercise>().UpdateRange(progressions);
                }

                var newEquipment = await _context.Equipment
                    .Where(e => viewModel.EquipmentBinder != null && viewModel.EquipmentBinder.Contains(e.Id))
                    .ToListAsync();
                _context.TryUpdateManyToMany(viewModel.User.UserEquipments, newEquipment.Select(e =>
                    new UserEquipment()
                    {
                        EquipmentId = e.Id,
                        UserId = viewModel.User.Id
                    }),
                    x => x.EquipmentId
                );

                _context.TryUpdateManyToMany(viewModel.User.UserFrequencies, // Remove all
                    viewModel.UserFrequencies
                        .Where(f => !f.Hide)
                        // At least some muscle groups or movement patterns are being worked
                        .Where(f => f.MuscleGroups != MuscleGroups.None || f.MovementPatterns != MovementPattern.None)
                        .Select(e => new UserFrequency()
                        {
                            UserId = viewModel.User.Id,
                            Id = e.Day,
                            Rotation = new Entities.Newsletter.NewsletterRotation(e.Day, e.MuscleGroups, e.MovementPatterns),
                        }),
                    x => x.Id, currNext => currNext.First.Rotation = currNext.Second.Rotation
                );

                viewModel.User.EmailVerbosity = viewModel.EmailVerbosity;
                viewModel.User.FootnoteType = viewModel.FootnoteType;
                viewModel.User.PrehabFocus = viewModel.PrehabFocus;
                viewModel.User.RehabFocus = viewModel.RehabFocus;
                viewModel.User.DeloadAfterEveryXWeeks = viewModel.DeloadAfterEveryXWeeks;
                viewModel.User.RefreshAccessoryEveryXWeeks = viewModel.RefreshAccessoryEveryXWeeks;
                viewModel.User.RefreshFunctionalEveryXWeeks = viewModel.RefreshFunctionalEveryXWeeks;
                viewModel.User.SportsFocus = viewModel.SportsFocus;
                viewModel.User.EmailAtUTCOffset = viewModel.EmailAtUTCOffset;
                viewModel.User.SendDays = viewModel.SendDays;
                viewModel.User.StretchingMuscles = viewModel.StretchingMuscles;
                viewModel.User.IsNewToFitness = viewModel.IsNewToFitness;
                viewModel.User.PreferStaticImages = viewModel.PreferStaticImages;
                viewModel.User.IntensityLevel = viewModel.IntensityLevel;
                viewModel.User.Frequency = viewModel.Frequency;
                viewModel.User.OffDayStretching = viewModel.OffDayStretching;

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

        // User is enabling their account or preventing it from being disabled for inactivity.
        TempData[TempData_User.SuccessMessage] = "Thank you!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
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
    [Route("variation/ignore"), HttpGet]
    public async Task<IActionResult> IgnoreVariation(string email, int exerciseId, int variationId, string token)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(e => e.UserId == user.Id && e.ExerciseId == exerciseId);
        var userExerciseVariation = await _context.UserExerciseVariations
            .FirstOrDefaultAsync(v => v.UserId == user.Id && v.ExerciseVariation.VariationId == variationId && v.ExerciseVariation.ExerciseId == exerciseId);
        var variation = await _context.Variations.FirstOrDefaultAsync(p => p.Id == variationId);
        var exercise = await _context.Exercises.FirstOrDefaultAsync(p => p.Id == exerciseId);

        // May be null if the variations was soft/hard deleted
        if (variation == null || exercise == null || userExercise == null || userExerciseVariation == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var exercises = (await new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
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

        var variations = (await new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithOrderBy(OrderBy.Progression)
            .WithExercises(x =>
            {
                x.AddVariations(new List<Variation>(1) { variation });
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main)
            {
                Verbosity = Models.Newsletter.Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            })
            .ToList();

        return View(new IgnoreVariationViewModel()
        {
            Email = email,
            Token = token,
            Variation = variation,
            Exercise = exercise,
            Exercises = exercises,
            Variations = variations,
            UserExercise = userExercise,
            UserExerciseVariation = userExerciseVariation
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

            userProgression.Ignore = true;
        }

        if (variationId != null)
        {
            var userVariationProgression = await _context.UserVariations
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

    [Route("variation/refresh"), HttpPost]
    public async Task<IActionResult> RefreshVariationPost(string email, string token, [FromForm] int? exerciseId = null, [FromForm] int? variationId = null)
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

            userProgression.RefreshAfter = null;
            userProgression.LastSeen = Today;
        }

        if (variationId != null)
        {
            var userVariationProgression = await _context.UserExerciseVariations
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseVariation.VariationId == variationId);

            // May be null if the exercise was soft/hard deleted
            if (userVariationProgression == null)
            {
                return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
            }

            userVariationProgression.RefreshAfter = null;
            userVariationProgression.LastSeen = Today;
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

    [Route("split/progress"), HttpPost]
    public async Task<IActionResult> AdvanceSplit(string email, string token)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Add a dummy newsletter to advance the workout split
        var nextNewsletterRotation = await _userService.GetTodaysNewsletterRotation(user);
        (var needsDeload, _) = await _userService.CheckNewsletterDeloadStatus(user);
        var newsletter = new Entities.Newsletter.Newsletter(Today, user, nextNewsletterRotation, user.Frequency, needsDeload);
        _context.Newsletters.Add(newsletter);

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your workout split has been advanced!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
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

    [Route("muscle/reset"), HttpPost]
    public async Task<IActionResult> ResetMuscleRanges(string email, string token, MuscleGroups? muscleGroup = null)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await _context.UserMuscles
            .Where(um => um.User.Id == user.Id)
            .Where(um => muscleGroup == null || um.MuscleGroup == muscleGroup)
            .ExecuteDeleteAsync();
       
        TempData[TempData_User.SuccessMessage] = "Your muscle targets have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [Route("muscle/start/decrease"), HttpPost]
    public async Task<IActionResult> DecreaseStartMuscleRange(string email, string token, MuscleGroups muscleGroup)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userMuscleGroup = await _context.UserMuscles.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
        if (userMuscleGroup == null)
        {
            _context.UserMuscles.Add(new UserMuscle() { 
                UserId = user.Id, 
                MuscleGroup = muscleGroup,
                Start = UserService.MuscleTargets[muscleGroup].Start.Value - UserService.IncrementMuscleTargetBy, 
                End = UserService.MuscleTargets[muscleGroup].End.Value
            });
        }
        else
        {
            userMuscleGroup.Start -= UserService.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserService.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [Route("muscle/start/increase"), HttpPost]
    public async Task<IActionResult> IncreaseStartMuscleRange(string email, string token, MuscleGroups muscleGroup)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userMuscleGroup = await _context.UserMuscles.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
        if (userMuscleGroup == null)
        {
            _context.UserMuscles.Add(new UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserService.MuscleTargets[muscleGroup].Start.Value + UserService.IncrementMuscleTargetBy,
                End = UserService.MuscleTargets[muscleGroup].End.Value
            });
        }
        else
        {
            userMuscleGroup.Start += UserService.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserService.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [Route("muscle/end/decrease"), HttpPost]
    public async Task<IActionResult> DecreaseEndMuscleRange(string email, string token, MuscleGroups muscleGroup)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userMuscleGroup = await _context.UserMuscles.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
        if (userMuscleGroup == null)
        {
            _context.UserMuscles.Add(new UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserService.MuscleTargets[muscleGroup].Start.Value,
                End = UserService.MuscleTargets[muscleGroup].End.Value - UserService.IncrementMuscleTargetBy
            });
        }
        else
        {
            userMuscleGroup.End -= UserService.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserService.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [Route("muscle/end/increase"), HttpPost]
    public async Task<IActionResult> IncreaseEndMuscleRange(string email, string token, MuscleGroups muscleGroup)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userMuscleGroup = await _context.UserMuscles.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
        if (userMuscleGroup == null)
        {
            _context.UserMuscles.Add(new UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserService.MuscleTargets[muscleGroup].Start.Value,
                End = UserService.MuscleTargets[muscleGroup].End.Value + UserService.IncrementMuscleTargetBy
            });
        }
        else
        {
            userMuscleGroup.End += UserService.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserService.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }
}
