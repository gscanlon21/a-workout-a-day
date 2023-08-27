﻿using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Options;
using Core.Models.User;
using Data;
using Data.Dtos.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.Footnote;
using Data.Entities.User;
using Data.Query.Builders;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Web.Code;
using Web.Code.TempData;
using Web.Controllers.Index;
using Web.ViewModels.User;

namespace Web.Controllers.User;

[Route($"u/{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 1)]
[Route($"user/{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 2)]
public class UserController : ViewController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "User";

    /// <summary>
    /// The reason for disabling the user's account when directed by the user.
    /// </summary>
    public const string UserDisabledByUserReason = "Disabled by user.";

    /// <summary>
    /// Message to show to the user when a link has expired.
    /// </summary>
    public const string LinkExpiredMessage = "This link has expired.";

    private readonly CoreContext _context;
    private readonly UserRepo _userRepo;
    private readonly IOptions<SiteSettings> _siteSettings;

    public UserController(CoreContext context, IOptions<SiteSettings> siteSettings, UserRepo userRepo) : base()
    {
        _context = context;
        _userRepo = userRepo;
        _siteSettings = siteSettings;
    }

    #region Edit User

    [HttpPost]
    [Route("footnote/add")]
    public async Task<IActionResult> AddFootnote(string email, string token, [FromForm] string note, [FromForm] string? source)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        _context.Add(new Footnote()
        {
            User = user,
            Note = note,
            Source = source,
            Type = FootnoteType.Custom
        });

        await _context.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("footnote/remove")]
    public async Task<IActionResult> RemoveFootnote(string email, string token, [FromForm] int footnoteId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await _context.Footnotes
            // The user has control of this footnote and is not a built-in footnote.
            .Where(f => f.UserId.HasValue && f.User == user)
            .Where(f => f.Id == footnoteId)
            .ExecuteDeleteAsync();

        await _context.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    private async Task<UserEditViewModel> PopulateUserEditViewModel(UserEditViewModel viewModel)
    {
        viewModel.UserFrequencies = (viewModel.UserFrequencies?.NullIfEmpty() ?? (await _userRepo.GetUpcomingRotations(viewModel.User, viewModel.User.Frequency)).OrderBy(f => f.Id).Select(f => new UserEditFrequencyViewModel(f))).ToList();
        while (viewModel.UserFrequencies.Count < UserConsts.MaxUserFrequencies)
        {
            viewModel.UserFrequencies.Add(new UserEditFrequencyViewModel() { Day = viewModel.UserFrequencies.Count + 1 });
        }

        foreach (var muscleGroup in UserMuscleMobility.MuscleTargets.Keys.OrderBy(mg => mg.GetSingleDisplayName()))
        {
            var userMuscleMobility = viewModel.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == muscleGroup);
            viewModel.UserMuscleMobilities.Add(userMuscleMobility != null ? new UserEditMuscleMobilityViewModel(userMuscleMobility) : new UserEditMuscleMobilityViewModel()
            {
                UserId = viewModel.User.Id,
                MuscleGroup = muscleGroup,
                Count = UserMuscleMobility.MuscleTargets.TryGetValue(muscleGroup, out int countTmp) ? countTmp : 0
            });
        }

        foreach (var muscleGroup in UserMuscleFlexibility.MuscleTargets.Keys.OrderBy(mg => mg.GetSingleDisplayName()))
        {
            var userMuscleFlexibility = viewModel.User.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == muscleGroup);
            viewModel.UserMuscleFlexibilities.Add(userMuscleFlexibility != null ? new UserEditMuscleFlexibilityViewModel(userMuscleFlexibility) : new UserEditMuscleFlexibilityViewModel()
            {
                UserId = viewModel.User.Id,
                MuscleGroup = muscleGroup,
                Count = UserMuscleFlexibility.MuscleTargets.TryGetValue(muscleGroup, out int countTmp) ? countTmp : 0
            });
        }

        viewModel.IgnoredExerciseBinder = viewModel.User.UserExercises.Where(ev => ev.Ignore).Select(e => e.ExerciseId).ToArray();
        viewModel.IgnoredExercises = viewModel.User.UserExercises
            .Where(ev => ev.Ignore)
            .Select(e => new SelectListItem()
            {
                Text = e.Exercise.Name,
                Value = e.ExerciseId.ToString(),
            })
            .OrderBy(e => e.Text)
            .ToList();

        viewModel.IgnoredExerciseVariationBinder = viewModel.User.UserExerciseVariations.Where(ev => ev.Ignore).Select(e => e.ExerciseVariationId).ToArray();
        viewModel.IgnoredExerciseVariations = viewModel.User.UserExerciseVariations
            .Where(ev => ev.Ignore)
            .Select(e => new SelectListItem()
            {
                Text = e.ExerciseVariation.Variation.Name + " - " + e.ExerciseVariation.ExerciseType.GetDisplayName32(EnumExtensions.DisplayNameType.ShortName),
                Value = e.ExerciseVariationId.ToString(),
            })
            .OrderBy(e => e.Text)
            .ToList();

        viewModel.IgnoredVariationBinder = viewModel.User.UserVariations.Where(ev => ev.Ignore).Select(e => e.VariationId).ToArray();
        viewModel.IgnoredVariations = viewModel.User.UserVariations
            .Where(ev => ev.Ignore)
            .Select(e => new SelectListItem()
            {
                Text = e.Variation.Name,
                Value = e.VariationId.ToString(),
            })
            .OrderBy(e => e.Text)
            .ToList();

        return viewModel;
    }

    /// <summary>
    /// Where the user edits their preferences.
    /// </summary>
    [HttpGet]
    [Route("", Order = 1)]
    [Route("e", Order = 2)]
    [Route("edit", Order = 3)]
    public async Task<IActionResult> Edit(string email, string token, bool? wasUpdated = null)
    {
        var user = await _userRepo.GetUser(email, token, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        return View("Edit", await PopulateUserEditViewModel(new UserEditViewModel(user, token)
        {
            WasUpdated = wasUpdated
        }));
    }

    [HttpPost]
    [Route("", Order = 1)]
    [Route("e", Order = 2)]
    [Route("edit", Order = 3)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string email, string token, UserEditViewModel viewModel)
    {
        if (token != viewModel.Token || email != viewModel.Email)
        {
            return NotFound();
        }

        viewModel.User = await _userRepo.GetUser(viewModel.Email, viewModel.Token, includeUserExerciseVariations: true, includeMuscles: true, includeFrequencies: true) ?? throw new ArgumentException(string.Empty, nameof(email));
        if (ModelState.IsValid)
        {
            try
            {
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

                // Ignored ExerciseVariations
                var oldUserExerciseVariationProgressions = await _context.UserExerciseVariations
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredExerciseVariationBinder == null || !viewModel.IgnoredExerciseVariationBinder.Contains(p.ExerciseVariationId))
                    .ToListAsync();
                var newUserExerciseVariationProgressions = await _context.UserExerciseVariations
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredExerciseVariationBinder != null && viewModel.IgnoredExerciseVariationBinder.Contains(p.ExerciseVariationId))
                    .ToListAsync();
                foreach (var oldUserExerciseVariationProgression in oldUserExerciseVariationProgressions)
                {
                    oldUserExerciseVariationProgression.Ignore = false;
                }
                foreach (var newUserExerciseVariationProgression in newUserExerciseVariationProgressions)
                {
                    newUserExerciseVariationProgression.Ignore = true;
                }
                _context.Set<UserExerciseVariation>().UpdateRange(oldUserExerciseVariationProgressions);
                _context.Set<UserExerciseVariation>().UpdateRange(newUserExerciseVariationProgressions);

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
                        progression.Progression = UserConsts.MinUserProgression;
                    }
                    _context.Set<UserExercise>().UpdateRange(progressions);
                }

                // If previous and current frequency is custom, allow editing of user frequencies.
                if (viewModel.User.Frequency == Frequency.Custom && viewModel.Frequency == Frequency.Custom)
                {
                    _context.UserFrequencies.RemoveRange(_context.UserFrequencies.Where(uf => uf.UserId == viewModel.User.Id));
                    _context.UserFrequencies.AddRange(viewModel.UserFrequencies
                        .Where(f => !f.Hide)
                        // At least some muscle groups or movement patterns are being worked
                        .Where(f => f.MuscleGroups != MuscleGroups.None || f.MovementPatterns != MovementPattern.None)
                        // Order before we index the items so only the days following blank rotatations shift ids
                        .OrderBy(f => f.Day)
                        .Select((e, i) => new UserFrequency()
                        {
                            // Using the index as the id so we don't have blank days if there is a rotation w/o muscle groups or movement patterns.
                            Id = i + 1,
                            UserId = viewModel.User.Id,
                            Rotation = new Data.Entities.Newsletter.WorkoutRotation(i + 1, e.MuscleGroups, e.MovementPatterns),
                        })
                    );
                }

                _context.UserMuscleMobilities.RemoveRange(_context.UserMuscleMobilities.Where(uf => uf.UserId == viewModel.User.Id));
                _context.UserMuscleMobilities.AddRange(viewModel.UserMuscleMobilities
                    .Select(umm => new UserMuscleMobility()
                    {
                        UserId = umm.UserId,
                        Count = umm.Count,
                        MuscleGroup = umm.MuscleGroup
                    })
                );

                _context.UserMuscleFlexibilities.RemoveRange(_context.UserMuscleFlexibilities.Where(uf => uf.UserId == viewModel.User.Id));
                _context.UserMuscleFlexibilities.AddRange(viewModel.UserMuscleFlexibilities
                    .Select(umm => new UserMuscleFlexibility()
                    {
                        UserId = umm.UserId,
                        Count = umm.Count,
                        MuscleGroup = umm.MuscleGroup
                    })
                );

                viewModel.User.Verbosity = viewModel.Verbosity;
                viewModel.User.Equipment = viewModel.Equipment;
                viewModel.User.FootnoteType = viewModel.FootnoteType;
                viewModel.User.DeloadAfterEveryXWeeks = viewModel.DeloadAfterEveryXWeeks;
                viewModel.User.RefreshAccessoryEveryXWeeks = viewModel.RefreshAccessoryEveryXWeeks;
                viewModel.User.RefreshFunctionalEveryXWeeks = viewModel.RefreshFunctionalEveryXWeeks;
                viewModel.User.SportsFocus = viewModel.SportsFocus;
                viewModel.User.SendDays = viewModel.SendDays;
                viewModel.User.SendHour = viewModel.SendHour;
                viewModel.User.ShowStaticImages = viewModel.ShowStaticImages;
                viewModel.User.Intensity = viewModel.Intensity;
                viewModel.User.Frequency = viewModel.Frequency;
                viewModel.User.IncludeMobilityWorkouts = viewModel.IncludeMobilityWorkouts;
                viewModel.User.IsNewToFitness = viewModel.IsNewToFitness;

                if (viewModel.User.NewsletterEnabled != viewModel.NewsletterEnabled)
                {
                    viewModel.User.NewsletterDisabledReason = viewModel.NewsletterEnabled ? null : UserDisabledByUserReason;
                }

                // If IncludeMobilityWorkouts is disabled, also remove any prehab or rehab focuses. Those are dependent on mobility workouts.
                if (viewModel.User.IncludeMobilityWorkouts)
                {
                    viewModel.User.PrehabFocus = viewModel.PrehabFocus;
                    viewModel.User.RehabFocus = viewModel.RehabFocus;
                }
                else
                {
                    viewModel.User.PrehabFocus = PrehabFocus.None;
                    viewModel.User.RehabFocus = RehabFocus.None;
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

        viewModel.WasUpdated = false;
        return View("Edit", await PopulateUserEditViewModel(viewModel));
    }

    #endregion
    #region Still Active Redirect

    /// <summary>
    /// Updates the user's LastActive date and redirects them to their final destination.
    /// </summary>
    [HttpGet]
    [Route("r", Order = 1)]
    [Route("redirect", Order = 2)]
    public async Task<IActionResult> IAmStillHere(string email, string token, string? to = null, string? redirectTo = null)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userIsConfirmingAccount = !user.LastActive.HasValue;
        user.LastActive = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(to))
        {
            return Redirect(to);
        }

        if (!string.IsNullOrWhiteSpace(redirectTo))
        {
            return Redirect(redirectTo);
        }

        // User is enabling their account or preventing it from being disabled for inactivity.
        TempData[TempData_User.SuccessMessage] = userIsConfirmingAccount
            ? "Thank you! Your first workout is on its way."
            : "Thank you! Take a moment to update your Workout Intensity to avoid adaptions.";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
    #region Manage Exercise

    /// <summary>
    /// Reduces the user's progression of an exercise.
    /// </summary>
    [HttpGet]
    [Route("e/{exerciseId}/r", Order = 1)]
    [Route("exercise/{exerciseId}/regress", Order = 2)]
    public async Task<IActionResult> ThatWorkoutWasTough(string email, int exerciseId, string token)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
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
            .FirstOrDefaultAsync() ?? UserConsts.MinUserProgression;

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
    /// Increases the user's progression of an exercise.
    /// </summary>
    [HttpGet]
    [Route("e/{exerciseId}/p", Order = 1)]
    [Route("exercise/{exerciseId}/progress", Order = 2)]
    public async Task<IActionResult> ThatWorkoutWasEasy(string email, int exerciseId, string token)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
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
            .FirstOrDefaultAsync() ?? UserConsts.MaxUserProgression;

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
    /// Reduces the user's progression of an exercise.
    /// </summary>
    [HttpGet]
    [Route("e/{exerciseId}/s", Order = 1)]
    [Route("exercise/{exerciseId}/suspend", Order = 2)]
    public async Task<IActionResult> SuspendExercise(string email, int exerciseId, string token)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userExercise = await _context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userExercise == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userExercise.LastSeen = Today.AddMonths(1);
        userExercise.RefreshAfter = null;
        await _context.SaveChangesAsync();

        return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. The exercise has been temporarily removed from your workouts.")
        {
            Demo = user.IsDemoUser
        });
    }


    #endregion
    #region Manage Exercise Variation

    /// <summary>
    /// Ignores a variation for a user.
    /// </summary>
    [HttpGet]
    [Route("ev/{exerciseVariationId}", Order = 1)]
    [Route("evercisevariation/{exerciseVariationId}", Order = 2)]
    public async Task<IActionResult> ManageExerciseVariation(string email, string token, int exerciseVariationId, bool? wasUpdated = null)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userExerciseVariation = await _context.UserExerciseVariations
            .Include(uev => uev.ExerciseVariation)
            .Where(uev => uev.UserId == user.Id)
            .FirstOrDefaultAsync(uev => uev.ExerciseVariationId == exerciseVariationId);

        // May be null if the variations was soft/hard deleted
        if (userExerciseVariation == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userExercise = await _context.UserExercises
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == userExerciseVariation.ExerciseVariation.ExerciseId);

        var userVariation = await _context.UserVariations
            .Include(uv => uv.Variation)
            .Where(uv => uv.UserId == user.Id)
            .FirstOrDefaultAsync(uv => uv.VariationId == userExerciseVariation.ExerciseVariation.VariationId);

        // May be null if the variations were soft/hard deleted
        if (userExercise == null || userVariation == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var exercises = (await new QueryBuilder()
            .WithExercises(x =>
            {
                x.AddExercises(new List<Data.Entities.Exercise.Exercise>(1) { userExercise.Exercise });
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, ExerciseDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var exerciseVariations = (await new QueryBuilder()
            .WithExercises(x =>
            {
                x.AddExerciseVariations(new List<ExerciseVariation>(1) { userExerciseVariation.ExerciseVariation });
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, ExerciseDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var variations = (await new QueryBuilder()
            .WithExercises(x =>
            {
                x.AddVariations(new List<Variation>(1) { userVariation.Variation });
            })
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, ExerciseDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        return View(new ManageExerciseVariationViewModel()
        {
            WasUpdated = wasUpdated,
            Email = email,
            Token = token,
            Variation = userVariation.Variation,
            ExerciseVariation = userExerciseVariation.ExerciseVariation,
            Exercise = userExercise.Exercise,
            Exercises = exercises,
            Variations = variations,
            ExerciseVariations = exerciseVariations,
            UserExercise = userExercise,
            UserExerciseVariation = userExerciseVariation,
            UserVariation = userVariation
        });
    }

    [HttpPost]
    [Route("ev/e/ignore", Order = 1)]
    [Route("exercisevariation/exercise/ignore", Order = 2)]
    public async Task<IActionResult> IgnoreExercise(string email, string token, [FromForm] int exerciseVariationId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == _context.ExerciseVariations.First(ev => ev.Id == exerciseVariationId).ExerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Ignore = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseVariationId, WasUpdated = true });
    }

    [HttpPost]
    [Route("ev/e/refresh", Order = 1)]
    [Route("exercisevariation/exercise/refresh", Order = 2)]
    public async Task<IActionResult> RefreshExercise(string email, string token, [FromForm] int exerciseVariationId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await _context.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == _context.ExerciseVariations.First(ev => ev.Id == exerciseVariationId).ExerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.RefreshAfter = null;
        userProgression.LastSeen = Today;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseVariationId, WasUpdated = true });
    }

    [HttpPost]
    [Route("ev/v/ignore", Order = 1)]
    [Route("exercisevariation/variation/ignore", Order = 2)]
    public async Task<IActionResult> IgnoreVariation(string email, string token, [FromForm] int exerciseVariationId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariationProgression = await _context.UserVariations
            .Where(uv => uv.UserId == user.Id)
            .FirstOrDefaultAsync(uv => uv.VariationId == _context.ExerciseVariations.First(ev => ev.Id == exerciseVariationId).VariationId);

        // May be null if the exercise was soft/hard deleted
        if (userVariationProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariationProgression.Ignore = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseVariationId, WasUpdated = true });
    }

    [HttpPost]
    [Route("ev/ignore", Order = 1)]
    [Route("exercisevariation/ignore", Order = 2)]
    public async Task<IActionResult> IgnoreExerciseVariation(string email, string token, [FromForm] int exerciseVariationId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariationProgression = await _context.UserExerciseVariations
            .Where(uv => uv.UserId == user.Id)
            .FirstOrDefaultAsync(uv => uv.ExerciseVariationId == exerciseVariationId);

        // May be null if the exercise was soft/hard deleted
        if (userVariationProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariationProgression.Ignore = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseVariationId, WasUpdated = true });
    }

    [HttpPost]
    [Route("ev/refresh", Order = 1)]
    [Route("exercisevariation/refresh", Order = 2)]
    public async Task<IActionResult> RefreshExerciseVariation(string email, string token, [FromForm] int exerciseVariationId)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariationProgression = await _context.UserExerciseVariations
            .Where(uev => uev.UserId == user.Id)
            .FirstOrDefaultAsync(uev => uev.ExerciseVariationId == exerciseVariationId);

        // May be null if the exercise was soft/hard deleted
        if (userVariationProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariationProgression.RefreshAfter = null;
        userVariationProgression.LastSeen = Today;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseVariationId, WasUpdated = true });
    }

    #endregion
    #region Manage Variation

    /// <summary>
    /// Shows a form to the user where they can update their Pounds lifted.
    /// </summary>
    [HttpGet]
    [Route("v/{variationId}", Order = 1)]
    [Route("variation/{variationId}", Order = 2)]
    public async Task<IActionResult> ManageVariation(string email, int variationId, string token, bool? wasUpdated = null)
    {
        var user = await _userRepo.GetUser(email, token, allowDemoUser: true);
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

        var userWeights = await _context.UserVariationWeights
            .Where(uw => uw.UserId == user.Id)
            .Where(uw => uw.VariationId == variationId)
            .ToListAsync();
        return View(new UserManageVariationViewModel(userWeights, userProgression.Weight)
        {
            WasUpdated = wasUpdated,
            Token = token,
            Email = email,
            VariationId = variationId,
            Weight = userProgression.Weight,
            VariationName = (await _context.Variations.FirstAsync(v => v.Id == variationId)).Name
        });
    }

    [HttpPost]
    [Route("v/{variationId}", Order = 1)]
    [Route("variation/{variationId}", Order = 2)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageVariation(string email, string token, UserManageVariationViewModel viewModel)
    {
        if (token != viewModel.Token || email != viewModel.Email)
        {
            return NotFound();
        }

        viewModel.VariationName = (await _context.Variations.FirstAsync(v => v.Id == viewModel.VariationId)).Name;

        if (ModelState.IsValid)
        {
            var user = await _userRepo.GetUser(viewModel.Email, viewModel.Token);
            if (user == null)
            {
                return NotFound();
            }

            // Set the new weight on the UserVariation
            var userProgression = await _context.UserVariations
                .Include(p => p.Variation)
                .FirstAsync(p => p.UserId == user.Id && p.VariationId == viewModel.VariationId);
            userProgression.Weight = viewModel.Weight;

            // Log the weight as a UserWeight
            var todaysUserWeight = await _context.UserVariationWeights
                .Where(uw => uw.UserId == user.Id)
                .Where(uw => uw.VariationId == viewModel.VariationId)
                .FirstOrDefaultAsync(uw => uw.Date == Today);
            if (todaysUserWeight != null)
            {
                todaysUserWeight.Weight = userProgression.Weight;
            }
            else
            {
                _context.Add(new UserVariationWeight()
                {
                    Date = Today,
                    UserId = user.Id,
                    Weight = userProgression.Weight,
                    VariationId = viewModel.VariationId,
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(UserController.ManageVariation), new { email, token, viewModel.VariationId, WasUpdated = true });
        }

        return await ManageVariation(email, viewModel.VariationId, token, wasUpdated: false);
    }

    #endregion
    #region Workout Split

    [HttpPost]
    [Route("split/progress")]
    public async Task<IActionResult> AdvanceSplit(string email, string token)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Add a dummy newsletter to advance the workout split
        var (needsDeload, _) = await _userRepo.CheckNewsletterDeloadStatus(user);
        var rotation = (await _userRepo.GetUpcomingRotations(user, user.Frequency)).First();
        var newsletter = new Data.Entities.Newsletter.UserWorkout(Today, user, rotation, user.Frequency, needsDeload);
        _context.UserWorkouts.Add(newsletter);

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your workout split has been advanced!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
    #region MuscleRanges

    [HttpPost]
    [Route("muscle/reset")]
    public async Task<IActionResult> ResetMuscleRanges(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await _context.UserMuscleStrengths
            .Where(um => um.User.Id == user.Id)
            .Where(um => muscleGroups.HasFlag(um.MuscleGroup))
            .ExecuteDeleteAsync();

        TempData[TempData_User.SuccessMessage] = "Your muscle targets have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/start/decrease")]
    public async Task<IActionResult> DecreaseStartMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = Math.Max(UserMuscleStrength.MuscleTargetMin, UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value - UserConsts.IncrementMuscleTargetBy),
                    End = UserMuscleStrength.MuscleTargets[muscleGroup].End.Value
                });
            }
            else
            {
                userMuscleGroup.Start = Math.Max(UserMuscleStrength.MuscleTargetMin, userMuscleGroup.Start - UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/start/increase")]
    public async Task<IActionResult> IncreaseStartMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value + UserConsts.IncrementMuscleTargetBy,
                    End = UserMuscleStrength.MuscleTargets[muscleGroup].End.Value
                });
            }
            else
            {
                userMuscleGroup.Start = Math.Min(userMuscleGroup.End - UserConsts.IncrementMuscleTargetBy, userMuscleGroup.Start + UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/end/decrease")]
    public async Task<IActionResult> DecreaseEndMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value,
                    End = UserMuscleStrength.MuscleTargets[muscleGroup].End.Value - UserConsts.IncrementMuscleTargetBy
                });
            }
            else
            {
                userMuscleGroup.End = Math.Max(userMuscleGroup.Start + UserConsts.IncrementMuscleTargetBy, userMuscleGroup.End - UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/end/increase")]
    public async Task<IActionResult> IncreaseEndMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var muscleTargetMax = UserMuscleStrength.MuscleTargets.Values.MaxBy(v => v.End.Value).End.Value;
        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value,
                    End = Math.Min(muscleTargetMax, UserMuscleStrength.MuscleTargets[muscleGroup].End.Value + UserConsts.IncrementMuscleTargetBy)
                });
            }
            else
            {
                userMuscleGroup.End = Math.Min(muscleTargetMax, userMuscleGroup.End + UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
    #region Delete

    /// <summary>
    /// User delete confirmation page.
    /// </summary>
    [HttpGet]
    [Route("d", Order = 1)]
    [Route("delete", Order = 2)]
    public async Task<IActionResult> Delete(string email, string token)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        return View(new UserEditViewModel(user, token));
    }

    [HttpPost]
    [Route("d", Order = 1)]
    [Route("delete", Order = 2)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string email, string token)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user != null)
        {
            _context.UserWorkouts.RemoveRange(await _context.UserWorkouts.Where(n => n.UserId == user.Id).ToListAsync());
            _context.Users.Remove(user); // Will also remove from ExerciseUserProgressions and EquipmentUsers
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(IndexController.Index), IndexController.Name, new { WasUnsubscribed = true });
    }

    #endregion
    #region User Tokens

    [HttpPost]
    [Route("token/create")]
    public async Task<IActionResult> CreateToken(string email, string token)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Delete old app tokens
        await _context.UserTokens
            .Where(ut => ut.UserId == user.Id)
            .Where(ut => ut.Expires == DateTime.MaxValue)
            .ExecuteDeleteAsync();

        var newToken = await _userRepo.AddUserToken(user, DateTime.MaxValue);
        TempData[TempData_User.SuccessMessage] = $"Your new app token: {newToken}"; // For your security we wonʼt show this password again, so make sure youʼve got it right before you close this dialog.
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
}
