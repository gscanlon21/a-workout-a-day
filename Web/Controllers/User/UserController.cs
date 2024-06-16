using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.User;
using Data;
using Data.Entities.User;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.TempData;
using Web.ViewModels.Components.User;
using Web.ViewModels.User;

namespace Web.Controllers.User;

[Route($"u/{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 1)]
[Route($"user/{{email:regex({UserCreateViewModel.EmailRegex})}}", Order = 2)]
public partial class UserController(CoreContext context, UserRepo userRepo) : ViewController()
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

    /// <summary>
    /// Where the user edits their preferences.
    /// </summary>
    [HttpGet]
    [Route("", Order = 1)]
    [Route("e", Order = 2)]
    [Route("edit", Order = 3)]
    public async Task<IActionResult> Edit(string email = UserConsts.DemoUser, string token = UserConsts.DemoToken, bool? wasUpdated = null)
    {
        var user = await userRepo.GetUser(email, token, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        return View("Edit", new UserEditViewModel(user, token)
        {
            WasUpdated = wasUpdated
        });
    }

    [HttpPost]
    [Route("", Order = 1)]
    [Route("e", Order = 2)]
    [Route("edit", Order = 3)]
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

                viewModel.User.SendDays = viewModel.SendDays;
                viewModel.User.SendHour = viewModel.SendHour;
                viewModel.User.Intensity = viewModel.Intensity;
                viewModel.User.Frequency = viewModel.Frequency;
                viewModel.User.Verbosity = viewModel.Verbosity;
                viewModel.User.Equipment = viewModel.Equipment;
                viewModel.User.RehabFocus = viewModel.RehabFocus;
                viewModel.User.RehabSkills = viewModel.RehabSkills;
                viewModel.User.PrehabFocus = viewModel.PrehabFocus;
                viewModel.User.SportsFocus = viewModel.SportsFocus;
                viewModel.User.FootnoteType = viewModel.FootnoteType;
                viewModel.User.IsNewToFitness = viewModel.IsNewToFitness;
                viewModel.User.ShowStaticImages = viewModel.ShowStaticImages;
                viewModel.User.DeloadAfterXWeeks = viewModel.DeloadAfterXWeeks;
                viewModel.User.IncludeMobilityWorkouts = viewModel.IncludeMobilityWorkouts;

                if (viewModel.User.NewsletterEnabled != viewModel.NewsletterEnabled)
                {
                    viewModel.User.NewsletterDisabledReason = viewModel.NewsletterEnabled ? null : UserDisabledByUserReason;
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
        return View("Edit", viewModel);
    }

    #endregion
    #region Advanced Settings

    [HttpPost]
    [Route("e/a", Order = 1)]
    [Route("edit/advanced", Order = 2)]
    public async Task<IActionResult> EditAdvanced(string email, string token, AdvancedViewModel viewModel)
    {
        var user = await userRepo.GetUser(email, token, includeUserExerciseVariations: true, includeMuscles: true, includeFrequencies: true) ?? throw new ArgumentException(string.Empty, nameof(email));
        if (ModelState.IsValid)
        {
            try
            {
                user.AtLeastXUniqueMusclesPerExercise_Accessory = viewModel.AtLeastXUniqueMusclesPerExercise_Accessory;
                user.AtLeastXUniqueMusclesPerExercise_Mobility = viewModel.AtLeastXUniqueMusclesPerExercise_Mobility;
                user.AtLeastXUniqueMusclesPerExercise_Flexibility = viewModel.AtLeastXUniqueMusclesPerExercise_Flexibility;
                user.WeightIsolationXTimesMore = viewModel.WeightIsolationXTimesMore;
                user.WeightSecondaryMusclesXTimesLess = viewModel.WeightSecondaryMusclesXTimesLess;
                user.FootnoteCountTop = viewModel.FootnoteCountTop;
                user.FootnoteCountBottom = viewModel.FootnoteCountBottom;
                user.IgnorePrerequisites = viewModel.IgnorePrerequisites;

                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(context.Users?.Any(e => e.Email == email)).GetValueOrDefault())
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

        return RedirectToAction(nameof(Edit), new { email, token, WasUpdated = true });
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
        var newsletter = new Data.Entities.Newsletter.UserWorkout(Today, user, rotation, user.Frequency, user.Intensity, needsDeload);
        context.UserWorkouts.Add(newsletter);

        await context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your workout split has been advanced!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
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
