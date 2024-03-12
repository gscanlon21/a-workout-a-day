using Core.Consts;
using Core.Models.Newsletter;
using Data.Dtos.Newsletter;
using Data.Entities.User;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Web.Code;
using Web.ViewModels.User;

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
        var user = await userRepo.GetUser(email, token, allowDemoUser: true);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var parameters = new UserManageExerciseVariationViewModel.Parameters(section, email, token, exerciseId, variationId);
        var userVariation = await context.UserVariations
            .IgnoreQueryFilters()
            .Include(p => p.Variation)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == variationId && p.Section == section);

        // Variations are managed per section, so ignoring variations for .None sections that are only for managing exercises.
        UserManageVariationViewModel? variationViewModel = null;
        if (userVariation != null && section != Section.None)
        {
            var userWeights = await context.UserVariationWeights
                .Where(uw => uw.UserVariationId == userVariation.Id)
                .ToListAsync();

            var variations = (await new QueryBuilder(section)
                .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
                .WithExercises(x =>
                {
                    x.AddVariations([userVariation.Variation]);
                })
                .Build()
                .Query(serviceScopeFactory))
                .Select(r => new ExerciseVariationDto(r)
                .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
                .DistinctBy(vm => vm.Variation)
                .ToList();

            variationViewModel = new UserManageVariationViewModel(userWeights, userVariation.Weight)
            {
                Parameters = parameters,
                VariationSection = section,
                User = user,
                Weight = userVariation.Weight,
                Variation = userVariation.Variation,
                Variations = variations,
                UserVariation = userVariation,
            };
        }

        var userExercise = await context.UserExercises
            .IgnoreQueryFilters()
            .Include(ue => ue.Exercise)
            .Where(ue => ue.UserId == user.Id)
            .FirstOrDefaultAsync(ue => ue.ExerciseId == exerciseId);

        if (userExercise == null)
        {
            userExercise = new UserExercise()
            {
                UserId = user.Id,
                ExerciseId = exerciseId,
                Progression = user.IsNewToFitness ? UserConsts.MinUserProgression : UserConsts.MidUserProgression,
                Exercise = await context.Exercises.FirstAsync(e => e.Id == exerciseId),
            };

            context.UserExercises.Add(userExercise);
            await context.SaveChangesAsync();
        }

        var exercises = (await new QueryBuilder(Section.None)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddExercises([userExercise.Exercise]);
            })
            .Build()
            .Query(serviceScopeFactory))
            // Order by progression levels
            .OrderBy(vm => vm.Variation.Progression.Min)
            .ThenBy(vm => vm.Variation.Progression.Max == null)
            .ThenBy(vm => vm.Variation.Progression.Max)
            .ThenBy(vm => vm.Variation.Name)
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        var exerciseViewModel = new UserManageExerciseViewModel()
        {
            Parameters = parameters,
            User = user,
            Exercise = userExercise.Exercise,
            Exercises = exercises,
            UserExercise = userExercise,
        };

        return View(new UserManageExerciseVariationViewModel()
        {
            WasUpdated = wasUpdated,
            Exercise = exerciseViewModel,
            Variation = variationViewModel,
        });
    }

    /// <summary>
    /// Reduces the user's progression of an exercise.
    /// </summary>
    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/r", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/regress", Order = 2)]
    public async Task<IActionResult> ThatWorkoutWasTough(string email, string token, int exerciseId, int variationId, Section section)
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

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    /// <summary>
    /// Increases the user's progression of an exercise.
    /// </summary>
    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/p", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/progress", Order = 2)]
    public async Task<IActionResult> ThatWorkoutWasEasy(string email, string token, int exerciseId, int variationId, Section section)
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

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/ie", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/ignore-exercise", Order = 2)]
    public async Task<IActionResult> IgnoreExercise(string email, string token, int exerciseId, int variationId, Section section)
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

        userProgression.Ignore = !userProgression.Ignore;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/ip", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/is-primary", Order = 2)]
    public async Task<IActionResult> IsPrimary(string email, string token, int exerciseId, int variationId, Section section, bool? isPrimary)
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

        userProgression.IsPrimary = isPrimary;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/re", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/refresh-exercise", Order = 2)]
    public async Task<IActionResult> RefreshExercise(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await userRepo.GetUser(email, token, allowDemoUser: true);
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

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/iv", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/ignore-variation", Order = 2)]
    public async Task<IActionResult> IgnoreVariation(string email, string token, int exerciseId, int variationId, Section section)
    {
        var user = await userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var userVariationProgression = await context.UserVariations
            .Where(uv => uv.UserId == user.Id)
            .FirstOrDefaultAsync(uv => uv.VariationId == variationId && uv.Section == section);

        // May be null if the exercise was soft/hard deleted
        if (userVariationProgression == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        userVariationProgression.Ignore = !userVariationProgression.Ignore;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
    }

    [HttpPost]
    [Route("{section:section}/{exerciseId}/{variationId}/l", Order = 1)]
    [Route("{section:section}/{exerciseId}/{variationId}/log", Order = 2)]
    public async Task<IActionResult> LogVariation(string email, string token, int exerciseId, int variationId, Section section, [Range(0, 999)] int weight)
    {
        if (ModelState.IsValid)
        {
            var user = await userRepo.GetUser(email, token, allowDemoUser: true);
            if (user == null)
            {
                return NotFound();
            }

            // Set the new weight on the UserVariation
            var userVariation = await context.UserVariations
                .Include(p => p.Variation)
                .FirstAsync(p => p.UserId == user.Id && p.VariationId == variationId && p.Section == section);
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

            return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = true });
        }

        return RedirectToAction(nameof(ManageExerciseVariation), new { email, token, exerciseId, variationId, section, WasUpdated = false });
    }
}
