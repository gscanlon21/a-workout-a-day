using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
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
public class UserController(CoreContext context, IServiceScopeFactory serviceScopeFactory, UserRepo userRepo) : ViewController()
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

    #region Edit User

    [HttpPost]
    [Route("footnote/add")]
    public async Task<IActionResult> AddFootnote(string email, string token, [FromForm] string note, [FromForm] string? source)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        context.Add(new Footnote()
        {
            User = user,
            Note = note,
            Source = source,
            Type = FootnoteType.Custom
        });

        await context.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("footnote/remove")]
    public async Task<IActionResult> RemoveFootnote(string email, string token, [FromForm] int footnoteId)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await context.Footnotes
            // The user has control of this footnote and is not a built-in footnote.
            .Where(f => f.UserId.HasValue && f.User == user)
            .Where(f => f.Id == footnoteId)
            .ExecuteDeleteAsync();

        await context.SaveChangesAsync();

        TempData[TempData_User.SuccessMessage] = "Your footnotes have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    private async Task<UserEditViewModel> PopulateUserEditViewModel(UserEditViewModel viewModel)
    {
        viewModel.UserFrequencies = (viewModel.UserFrequencies?.NullIfEmpty() ?? (await userRepo.GetUpcomingRotations(viewModel.User, viewModel.User.Frequency)).OrderBy(f => f.Id).Select(f => new UserEditFrequencyViewModel(f))).ToList();
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

        viewModel.IgnoredVariationBinder = viewModel.User.UserVariations.Where(ev => ev.Ignore).Select(e => e.VariationId).ToArray();
        viewModel.IgnoredVariations = viewModel.User.UserVariations
            .Where(ev => ev.Ignore)
            .Select(e => new SelectListItem()
            {
                Text = $"{e.Section.GetSingleDisplayName(EnumExtensions.DisplayNameType.ShortName)}: {e.Variation.Name}",
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
        var user = await userRepo.GetUser(email, token, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true);
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

        viewModel.User = await userRepo.GetUser(viewModel.Email, viewModel.Token, includeUserExerciseVariations: true, includeMuscles: true, includeFrequencies: true) ?? throw new ArgumentException(string.Empty, nameof(email));
        if (ModelState.IsValid)
        {
            try
            {
                // Ignored Exercises
                var oldUserProgressions = await context.UserExercises
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredExerciseBinder == null || !viewModel.IgnoredExerciseBinder.Contains(p.ExerciseId))
                    .ToListAsync();
                foreach (var oldUserProgression in oldUserProgressions)
                {
                    oldUserProgression.Ignore = false;
                }
                context.Set<UserExercise>().UpdateRange(oldUserProgressions);

                // Ignored Variations
                var oldUserVariationProgressions = await context.UserVariations
                    .Where(p => p.UserId == viewModel.User.Id)
                    .Where(p => viewModel.IgnoredVariationBinder == null || !viewModel.IgnoredVariationBinder.Contains(p.VariationId))
                    .ToListAsync();
                foreach (var oldUserVariationProgression in oldUserVariationProgressions)
                {
                    oldUserVariationProgression.Ignore = false;
                }
                context.Set<UserVariation>().UpdateRange(oldUserVariationProgressions);

                var rehabMuscleGroup = viewModel.RehabFocus.As<MuscleGroups>();
                if (rehabMuscleGroup != MuscleGroups.None && viewModel.User.RehabFocus != viewModel.RehabFocus)
                {
                    // If any exercise's variation's muscle is worked by the (new) recovery muscle, lower it's progression level and un-ignore it.
                    var progressions = context.UserExercises
                        .Where(up => up.UserId == viewModel.User.Id)
                        .Where(up => up.Exercise.Variations.Any(v => v.StrengthMuscles.HasFlag(rehabMuscleGroup)));
                    foreach (var progression in progressions)
                    {
                        progression.Ignore = false;
                        progression.Progression = UserConsts.MinUserProgression;
                    }
                    context.Set<UserExercise>().UpdateRange(progressions);
                }

                // If previous and current frequency is custom, allow editing of user frequencies.
                if (viewModel.User.Frequency == Frequency.Custom && viewModel.Frequency == Frequency.Custom)
                {
                    context.UserFrequencies.RemoveRange(context.UserFrequencies.Where(uf => uf.UserId == viewModel.User.Id));
                    context.UserFrequencies.AddRange(viewModel.UserFrequencies
                        .Where(f => !f.Hide)
                        // At least some muscle groups or movement patterns are being worked
                        .Where(f => f.MuscleGroups?.Any() == true || f.MovementPatterns != MovementPattern.None)
                        // Order before we index the items so only the days following blank rotatations shift ids
                        .OrderBy(f => f.Day)
                        .Select((e, i) => new UserFrequency()
                        {
                            // Using the index as the id so we don't have blank days if there is a rotation w/o muscle groups or movement patterns.
                            Id = i + 1,
                            UserId = viewModel.User.Id,
                            Rotation = new Data.Entities.Newsletter.WorkoutRotation(i + 1)
                            {
                                MuscleGroups = e.MuscleGroups!,
                                MovementPatterns = e.MovementPatterns
                            },
                        })
                    );
                }

                context.UserMuscleMobilities.RemoveRange(context.UserMuscleMobilities.Where(uf => uf.UserId == viewModel.User.Id));
                context.UserMuscleMobilities.AddRange(viewModel.UserMuscleMobilities
                    .Select(umm => new UserMuscleMobility()
                    {
                        UserId = umm.UserId,
                        Count = umm.Count,
                        MuscleGroup = umm.MuscleGroup
                    })
                );

                context.UserMuscleFlexibilities.RemoveRange(context.UserMuscleFlexibilities.Where(uf => uf.UserId == viewModel.User.Id));
                context.UserMuscleFlexibilities.AddRange(viewModel.UserMuscleFlexibilities
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

                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(context.Users?.Any(e => e.Email == viewModel.Email)).GetValueOrDefault())
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
        var user = await userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userIsConfirmingAccount = !user.LastActive.HasValue;
        user.LastActive = DateOnly.FromDateTime(DateTime.UtcNow);
        await context.SaveChangesAsync();

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
        var user = await userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Progression = await
            // Stop at the lower bounds of variations
            context.Variations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Min)
            // Stop at the upper bounds of variations
            .Union(context.Variations
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
            await context.SaveChangesAsync();
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
        var user = await userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Progression = await
            // Stop at the lower bounds of variations
            context.Variations
                .Where(ev => ev.ExerciseId == exerciseId)
                .Select(ev => ev.Progression.Min)
            // Stop at the upper bounds of variations
            .Union(context.Variations
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
            await context.SaveChangesAsync();
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
        var user = await userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userExercise = await context.UserExercises
            .Include(p => p.Exercise)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.ExerciseId == exerciseId);

        // May be null if the exercise was soft/hard deleted
        if (userExercise == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userExercise.LastSeen = Today.AddMonths(1);
        userExercise.RefreshAfter = null;
        await context.SaveChangesAsync();

        return View("StatusMessage", new StatusMessageViewModel($"Your preferences have been saved. The exercise has been temporarily removed from your workouts.")
        {
            Demo = user.IsDemoUser
        });
    }


    #endregion
    #region Manage Variation

    /// <summary>
    /// Shows a form to the user where they can update their Pounds lifted.
    /// </summary>
    [HttpGet]
    [Route("v/{variationId}/{section}", Order = 1)]
    [Route("variation/{variationId}/{section}", Order = 2)]
    public async Task<IActionResult> ManageVariation(string email, string token, int variationId, Section section, bool? wasUpdated = null)
    {
        var user = await userRepo.GetUser(email, token, allowDemoUser: false);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariation = await context.UserVariations
            .Include(p => p.Variation)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == variationId && p.Section.HasFlag(section));

        // May be null if the exercise was soft/hard deleted
        if (userVariation == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userWeights = await context.UserVariationWeights
            .Where(uw => uw.UserVariationId == userVariation.Id)
            .ToListAsync();

        var userExercise = await context.UserExercises
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == userVariation.Variation.ExerciseId);

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
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var variations = (await new QueryBuilder()
            .WithExercises(x =>
            {
                x.AddVariations(new List<Variation>(1) { userVariation.Variation });
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();


        return View(new UserManageVariationViewModel(userWeights, userVariation.Weight)
        {
            Section = section,
            WasUpdated = wasUpdated,
            Token = token,
            Email = email,
            VariationId = variationId,
            Weight = userVariation.Weight,
            Variation = userVariation.Variation,
            Exercise = userExercise.Exercise,
            Exercises = exercises,
            Variations = variations,
            UserExercise = userExercise,
            UserVariation = userVariation,
            VariationName = (await context.Variations.FirstAsync(v => v.Id == variationId)).Name
        });
    }

    [HttpPost]
    [Route("e/i", Order = 1)]
    [Route("exercise/ignore", Order = 2)]
    public async Task<IActionResult> IgnoreExercise(string email, string token, int variationId, Section section)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await context.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.Exercise.Variations.Any(v => v.Id == variationId));

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.Ignore = true;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageVariation), new { email, token, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("e/r", Order = 1)]
    [Route("exercise/refresh", Order = 2)]
    public async Task<IActionResult> RefreshExercise(string email, string token, int variationId, Section section)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userProgression = await context.UserExercises
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.Exercise.Variations.Any(v => v.Id == variationId));

        // May be null if the exercise was soft/hard deleted
        if (userProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userProgression.RefreshAfter = null;
        userProgression.LastSeen = Today;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageVariation), new { email, token, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("v/i", Order = 1)]
    [Route("variation/ignore", Order = 2)]
    public async Task<IActionResult> IgnoreVariation(string email, string token, int variationId, Section section)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariationProgression = await context.UserVariations
            .Where(uv => uv.UserId == user.Id)
            .FirstOrDefaultAsync(uv => uv.VariationId == variationId && uv.Section.HasFlag(section));

        // May be null if the exercise was soft/hard deleted
        if (userVariationProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariationProgression.Ignore = true;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageVariation), new { email, token, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("v/r", Order = 1)]
    [Route("variation/refresh", Order = 2)]
    public async Task<IActionResult> RefreshVariation(string email, string token, int variationId, Section section)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariation = await context.UserVariations
            .Where(uev => uev.UserId == user.Id)
            .FirstOrDefaultAsync(uev => uev.VariationId == variationId && uev.Section.HasFlag(section));

        // May be null if the exercise was soft/hard deleted
        if (userVariation == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariation.RefreshAfter = null;
        userVariation.LastSeen = Today;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageVariation), new { email, token, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("v/l", Order = 1)]
    [Route("variation/log", Order = 2)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogVariation(string email, string token, int variationId, Section section, [Range(0, 999)] int weight)
    {
        if (ModelState.IsValid)
        {
            var user = await userRepo.GetUser(email, token);
            if (user == null)
            {
                return NotFound();
            }

            // Set the new weight on the UserVariation
            var userVariation = await context.UserVariations
                .Include(p => p.Variation)
                .FirstAsync(p => p.UserId == user.Id && p.VariationId == variationId && p.Section.HasFlag(section));
            userVariation.Weight = weight;

            // Log the weight as a UserWeight
            var todaysUserWeight = await context.UserVariationWeights
                .Where(uw => uw.UserVariationId == userVariation.Id)
                .FirstOrDefaultAsync(uw => uw.Date == Today);
            if (todaysUserWeight != null)
            {
                todaysUserWeight.Weight = userVariation.Weight;
            }
            else
            {
                context.Add(new UserVariationWeight()
                {
                    Date = Today,
                    UserVariationId = userVariation.Id,
                    Weight = userVariation.Weight,
                });
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageVariation), new { email, token, variationId, section, WasUpdated = true });
        }

        return RedirectToAction(nameof(ManageVariation), new { email, token, variationId, section, WasUpdated = false });
    }

    #endregion
    #region Workout Split

    [HttpPost]
    [Route("split/progress")]
    public async Task<IActionResult> AdvanceSplit(string email, string token)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Add a dummy newsletter to advance the workout split
        var (needsDeload, _) = await userRepo.CheckNewsletterDeloadStatus(user);
        var rotation = (await userRepo.GetUpcomingRotations(user, user.Frequency)).First();
        var newsletter = new Data.Entities.Newsletter.UserWorkout(Today, user, rotation, user.Frequency, needsDeload);
        context.UserWorkouts.Add(newsletter);

        await context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your workout split has been advanced!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
    #region MuscleRanges

    [HttpPost]
    [Route("muscle/reset")]
    public async Task<IActionResult> ResetMuscleRanges(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await context.UserMuscleStrengths
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
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                context.UserMuscleStrengths.Add(new UserMuscleStrength()
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
                    context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/start/increase")]
    public async Task<IActionResult> IncreaseStartMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                context.UserMuscleStrengths.Add(new UserMuscleStrength()
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
                    context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/end/decrease")]
    public async Task<IActionResult> DecreaseEndMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                context.UserMuscleStrengths.Add(new UserMuscleStrength()
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
                    context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/end/increase")]
    public async Task<IActionResult> IncreaseEndMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MuscleGroups muscleGroups)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var muscleTargetMax = UserMuscleStrength.MuscleTargets.Values.MaxBy(v => v.End.Value).End.Value;
        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                context.UserMuscleStrengths.Add(new UserMuscleStrength()
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
                    context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await context.SaveChangesAsync();
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
        var user = await userRepo.GetUser(email, token);
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
        var user = await userRepo.GetUser(email, token);
        if (user != null)
        {
            context.UserWorkouts.RemoveRange(await context.UserWorkouts.Where(n => n.UserId == user.Id).ToListAsync());
            context.Users.Remove(user); // Will also remove from ExerciseUserProgressions and EquipmentUsers
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(IndexController.Index), IndexController.Name, new { WasUnsubscribed = true });
    }

    #endregion
    #region User Tokens

    [HttpPost]
    [Route("token/create")]
    public async Task<IActionResult> CreateToken(string email, string token)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Delete old app tokens
        await context.UserTokens
            .Where(ut => ut.UserId == user.Id)
            .Where(ut => ut.Expires == DateTime.MaxValue)
            .ExecuteDeleteAsync();

        var newToken = await userRepo.AddUserToken(user, DateTime.MaxValue);
        TempData[TempData_User.SuccessMessage] = $"Your new app token: {newToken}"; // For your security we wonʼt show this password again, so make sure youʼve got it right before you close this dialog.
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
}
