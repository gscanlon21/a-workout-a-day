using Core.Code.Extensions;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.Options;
using Data.Data;
using Data.Data.Query;
using Data.Entities.Exercise;
using Data.Entities.User;
using Microsoft.AspNetCore.Mvc;
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
    private readonly Data.Repos.UserRepo _userService;
    private readonly IOptions<SiteSettings> _siteSettings;

    public UserController(CoreContext context, IOptions<SiteSettings> siteSettings, Data.Repos.UserRepo userService) : base()
    {
        _context = context;
        _userService = userService;
        _siteSettings = siteSettings;
    }

    #region Edit User

    private async Task<UserEditViewModel> PopulateUserEditViewModel(UserEditViewModel viewModel)
    {
        viewModel.UserFrequencies = (viewModel.UserFrequencies?.NullIfEmpty() ?? (await _userService.GetCurrentAndUpcomingRotations(viewModel.User)).OrderBy(f => f.Id).Select(f => new UserEditFrequencyViewModel(f))).ToList();
        while (viewModel.UserFrequencies.Count < UserFrequency.MaxPerUser)
        {
            viewModel.UserFrequencies.Add(new UserEditFrequencyViewModel() { Day = viewModel.UserFrequencies.Count + 1 });
        }

        viewModel.EquipmentBinder = viewModel.User.UserEquipments.Select(e => e.EquipmentId).ToArray();
        viewModel.Equipment = await _context.Equipment
            .Where(e => e.DisabledReason == null)
            .OrderBy(e => e.Name)
            .ToListAsync();

        viewModel.IgnoredExerciseBinder = viewModel.User.UserExercises?.Where(ep => ep.Ignore).Select(e => e.ExerciseId).ToArray();
        viewModel.IgnoredExercises = await _context.Exercises
            .Where(e => viewModel.User.UserExercises != null && viewModel.User.UserExercises.Select(ep => ep.ExerciseId).Contains(e.Id))
            .OrderBy(e => e.Name)
            .ToListAsync();

        viewModel.IgnoredVariationBinder = viewModel.User.UserVariations?.Where(ep => ep.Ignore).Select(e => e.VariationId).ToArray();
        viewModel.IgnoredVariations = await _context.Variations
            .Where(e => viewModel.User.UserVariations != null && viewModel.User.UserVariations.Select(ep => ep.VariationId).Contains(e.Id))
            .OrderBy(e => e.Name)
            .ToListAsync();

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
            .Select(r => new Data.Models.Newsletter.ExerciseModel(r.User, r.Exercise, r.Variation, r.ExerciseVariation,
                  r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                  easierVariation: r.EasierVariation, harderVariation: r.HarderVariation,
                  intensityLevel: null, ExerciseTheme.Main)
            {
                Verbosity = Verbosity.Minimal,
                IntensityLevel = (IntensityLevel?)(IntensityLevel)(-1)
            }.AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Models.Newsletter.ExerciseModel>()!)
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
            .Select(r => new Data.Models.Newsletter.ExerciseModel(r.User, r.Exercise, r.Variation, r.ExerciseVariation,
              r.UserExercise, r.UserExerciseVariation, r.UserVariation,
              easierVariation: r.EasierVariation, harderVariation: r.HarderVariation,
              intensityLevel: null, ExerciseTheme.Main)
            {
                Verbosity = Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            }.AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Models.Newsletter.ExerciseModel>()!)
            .ToList();

        return viewModel;
    }

    /// <summary>
    /// Where the user edits their preferences.
    /// </summary>
    [HttpGet]
    [Route("", Order = 1)]
    [Route("edit", Order = 2)]
    public async Task<IActionResult> Edit(string email, string token, bool? wasUpdated = null)
    {
        var user = await _userService.GetUser(email, token, includeUserEquipments: true, includeUserExerciseVariations: true, includeMuscles: true, includeFrequencies: true);
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
    [Route("edit", Order = 2)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string email, string token, UserEditViewModel viewModel)
    {
        if (token != viewModel.Token || email != viewModel.Email)
        {
            return NotFound();
        }

        viewModel.User = await _userService.GetUser(viewModel.Email, viewModel.Token, includeUserEquipments: true, includeUserExerciseVariations: true, includeMuscles: true, includeFrequencies: true) ?? throw new ArgumentException(string.Empty, nameof(email));
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

                _context.UserEquipments.RemoveRange(_context.UserEquipments.Where(ue => ue.UserId == viewModel.User.Id));
                if (viewModel.EquipmentBinder != null)
                {
                    _context.UserEquipments.AddRange(viewModel.EquipmentBinder.Select(eId =>
                        new UserEquipment()
                        {
                            EquipmentId = eId,
                            UserId = viewModel.User.Id
                        })
                    );
                }

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
                        Rotation = new Data.Entities.Newsletter.NewsletterRotation(i + 1, e.MuscleGroups, e.MovementPatterns),
                    })
                );

                viewModel.User.EmailVerbosity = viewModel.EmailVerbosity;
                viewModel.User.FootnoteType = viewModel.FootnoteType;
                viewModel.User.PrehabFocus = viewModel.PrehabFocus;
                viewModel.User.RehabFocus = viewModel.RehabFocus;
                viewModel.User.DeloadAfterEveryXWeeks = viewModel.DeloadAfterEveryXWeeks;
                viewModel.User.RefreshAccessoryEveryXWeeks = viewModel.RefreshAccessoryEveryXWeeks;
                viewModel.User.RefreshFunctionalEveryXWeeks = viewModel.RefreshFunctionalEveryXWeeks;
                viewModel.User.SportsFocus = viewModel.SportsFocus;
                viewModel.User.SendDays = viewModel.SendDays;
                viewModel.User.SendHour = viewModel.SendHour;
                viewModel.User.MobilityMuscles = viewModel.MobilityMuscles;
                viewModel.User.IsNewToFitness = viewModel.IsNewToFitness;
                viewModel.User.ShowStaticImages = viewModel.ShowStaticImages;
                viewModel.User.IntensityLevel = viewModel.IntensityLevel;
                viewModel.User.Frequency = viewModel.Frequency;
                viewModel.User.SendMobilityWorkouts = viewModel.SendMobilityWorkouts;

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

        if (to != null)
        {
            return Redirect(to);
        }

        if (redirectTo != null)
        {
            return Redirect(redirectTo);
        }

        // User is enabling their account or preventing it from being disabled for inactivity.
        TempData[TempData_User.SuccessMessage] = "Thank you!";
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
    /// Increases the user's progression of an exercise.
    /// </summary>
    [HttpGet]
    [Route("e/{exerciseId}/p", Order = 1)]
    [Route("exercise/{exerciseId}/progress", Order = 2)]
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
        var user = await _userService.GetUser(email, token);
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

        var userExercise = await _context.UserExercises.FirstOrDefaultAsync(e => e.UserId == user.Id && e.ExerciseId == userExerciseVariation.ExerciseVariation.ExerciseId);
        var userVariation = await _context.UserVariations.FirstOrDefaultAsync(e => e.UserId == user.Id && e.VariationId == userExerciseVariation.ExerciseVariation.VariationId);
        var exercise = await _context.Exercises.FirstOrDefaultAsync(p => p.Id == userExerciseVariation.ExerciseVariation.ExerciseId);
        var variation = await _context.Variations.FirstOrDefaultAsync(p => p.Id == userExerciseVariation.ExerciseVariation.VariationId);

        // May be null if the variations were soft/hard deleted
        if (variation == null || exercise == null || userExercise == null || userVariation == null)
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
                x.AddExercises(new List<Data.Entities.Exercise.Exercise>(1) { exercise });
            })
            .Build()
            .Query())
            .Select(r => new Data.Models.Newsletter.ExerciseModel(r.User, r.Exercise, r.Variation, r.ExerciseVariation,
              r.UserExercise, r.UserExerciseVariation, r.UserVariation,
              easierVariation: r.EasierVariation, harderVariation: r.HarderVariation,
              intensityLevel: null, ExerciseTheme.Main)
            {
                Verbosity = Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            }.AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Models.Newsletter.ExerciseModel>()!)
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
            .Select(r => new Data.Models.Newsletter.ExerciseModel(r, ExerciseTheme.Main)
            {
                Verbosity = Verbosity.Minimal,
                IntensityLevel = (IntensityLevel)(-1)
            }.AsType<Lib.ViewModels.Newsletter.ExerciseViewModel, Data.Models.Newsletter.ExerciseModel>()!)
            .ToList();

        return View(new ManageExerciseVariationViewModel()
        {
            WasUpdated = wasUpdated,
            Email = email,
            Token = token,
            Variation = variation,
            Exercise = exercise,
            Exercises = exercises,
            Variations = variations,
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
        var user = await _userService.GetUser(email, token);
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
        var user = await _userService.GetUser(email, token);
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
        var user = await _userService.GetUser(email, token);
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
    [Route("ev/refresh", Order = 1)]
    [Route("exercisevariation/refresh", Order = 2)]
    public async Task<IActionResult> RefreshExerciseVariation(string email, string token, [FromForm] int exerciseVariationId)
    {
        var user = await _userService.GetUser(email, token);
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

        return View(new UserManageVariationViewModel()
        {
            WasUpdated = wasUpdated,
            Token = token,
            Email = email,
            VariationId = variationId,
            Pounds = userProgression.Pounds,
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
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Add a dummy newsletter to advance the workout split
        var nextNewsletterRotation = await _userService.GetTodaysNewsletterRotation(user);
        (var needsDeload, _) = await _userService.CheckNewsletterDeloadStatus(user);
        var newsletter = new Data.Entities.Newsletter.Newsletter(Today, user, nextNewsletterRotation, user.Frequency, needsDeload);
        _context.Newsletters.Add(newsletter);

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your workout split has been advanced!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
    #region MuscleRanges

    [HttpPost]
    [Route("muscle/reset")]
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

    [HttpPost]
    [Route("muscle/start/decrease")]
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
            _context.UserMuscles.Add(new Data.Entities.User.UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserMuscle.MuscleTargets[muscleGroup].Start.Value - UserMuscle.IncrementMuscleTargetBy,
                End = UserMuscle.MuscleTargets[muscleGroup].End.Value
            });
        }
        else
        {
            userMuscleGroup.Start -= UserMuscle.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserMuscle.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/start/increase")]
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
            _context.UserMuscles.Add(new Data.Entities.User.UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserMuscle.MuscleTargets[muscleGroup].Start.Value + UserMuscle.IncrementMuscleTargetBy,
                End = UserMuscle.MuscleTargets[muscleGroup].End.Value
            });
        }
        else
        {
            userMuscleGroup.Start += UserMuscle.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserMuscle.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/end/decrease")]
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
            _context.UserMuscles.Add(new Data.Entities.User.UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserMuscle.MuscleTargets[muscleGroup].Start.Value,
                End = UserMuscle.MuscleTargets[muscleGroup].End.Value - UserMuscle.IncrementMuscleTargetBy
            });
        }
        else
        {
            userMuscleGroup.End -= UserMuscle.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserMuscle.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost]
    [Route("muscle/end/increase")]
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
            _context.UserMuscles.Add(new Data.Entities.User.UserMuscle()
            {
                UserId = user.Id,
                MuscleGroup = muscleGroup,
                Start = UserMuscle.MuscleTargets[muscleGroup].Start.Value,
                End = UserMuscle.MuscleTargets[muscleGroup].End.Value + UserMuscle.IncrementMuscleTargetBy
            });
        }
        else
        {
            userMuscleGroup.End += UserMuscle.IncrementMuscleTargetBy;

            // Delete this range so that any default updates take effect.
            if (userMuscleGroup.Range.Equals(UserMuscle.MuscleTargets[muscleGroup]))
            {
                _context.UserMuscles.Remove(userMuscleGroup);
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
        var user = await _userService.GetUser(email, token);
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
        var user = await _userService.GetUser(email, token);
        if (user != null)
        {
            _context.Newsletters.RemoveRange(await _context.Newsletters.Where(n => n.UserId == user.Id).ToListAsync());
            _context.Users.Remove(user); // Will also remove from ExerciseUserProgressions and EquipmentUsers
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(IndexController.Index), IndexController.Name, new { WasUnsubscribed = true });
    }

    #endregion
    #region User Tokens

    [HttpPost]
    [Route("token/create")]
    public async Task<IActionResult> CreateToken(string email, string token, MuscleGroups muscleGroup)
    {
        var user = await _userService.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Delete old app tokens
        await _context.UserTokens
            .Where(ut => ut.UserId == user.Id)
            .Where(ut => ut.Expires == DateOnly.MaxValue)
            .ExecuteDeleteAsync();

        var newToken = await _userService.AddUserToken(user, DateOnly.MaxValue);
        TempData[TempData_User.SuccessMessage] = $"Your new app token: {newToken}"; // For your security we wonʼt show this password again, so make sure youʼve got it right before you close this dialog.
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    #endregion
}
