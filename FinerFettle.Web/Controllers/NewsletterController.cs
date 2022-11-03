using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Entities.Newsletter;
using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Entities.Exercise;

namespace FinerFettle.Web.Controllers;

public class NewsletterController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Newsletter";

    /// <summary>
    /// Today's date from UTC
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NewsletterController(CoreContext context, IServiceScopeFactory serviceScopeFactory) : base(context) 
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    #region Helpers

    /// <summary>
    /// Grabs a user from an email address.
    /// </summary>
    private async Task<User> GetUser(string email, string token)
    {
        return await _context.Users
            // For displaying ignored exercises in the bottom of the newsletter
            .Include(u => u.UserExercises)
                .ThenInclude(ep => ep.Exercise)
            // For displaying user's equipment in the bottom of the newsletter
            .Include(u => u.UserEquipments) // Has a global query filter to filter out disabled equipment
                .ThenInclude(u => u.Equipment)
            .FirstAsync(u => u.Email == email && (u.UserTokens.Any(ut => ut.Token == token) || email == Entities.User.User.DemoUser));
    }

    /// <summary>
    /// Grabs the previous newsletter received by the user.
    /// </summary>
    private async Task<Newsletter?> GetPreviousNewsletter(User user)
    {
        return await _context.Newsletters
            .Where(n => n.User == user)
            .OrderBy(n => n.Date)
            .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            .LastOrDefaultAsync();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    private static NewsletterRotation GetTodaysNewsletterRotation(User user, Newsletter? previousNewsletter)
    {
        var todaysNewsletterRotation = new NewsletterTypeGroups(user.StrengtheningPreference).First(); // Have to start somewhere
        if (previousNewsletter != null)
        {
            todaysNewsletterRotation = new NewsletterTypeGroups(user.StrengtheningPreference)
                .SkipWhile(r => r != previousNewsletter.NewsletterRotation)
                .Skip(1)
                .FirstOrDefault() ?? todaysNewsletterRotation;
        }
        return todaysNewsletterRotation;
    }

    /// <summary>
    /// Checks if the user should deload for a week (reduce the intensity of their workout to reduce muscle growth stagnating).
    /// </summary>
    private async Task<bool> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.Newsletters
            .Where(n => n.User == user)
            .OrderBy(n => n.Date)
            .ThenBy(n => n.Id) // For testing/demo. When two newsletters are sent the same day, I want a different exercise set.
            .LastOrDefaultAsync(n => n.IsDeloadWeek)
                ?? await _context.Newsletters
                .Where(n => n.User == user)
                .OrderBy(n => n.Date)
                .ThenBy(n => n.Id) // For testing/demo. When two newsletters are sent the same day, I want a different exercise set.
                .FirstOrDefaultAsync(); // The oldest newsletter, for if there has never been a deload before.

        // Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
        bool needsDeload = lastDeload != null
            && (
                // Dates are the same week. Keep the deload going until the week is over.
                (lastDeload.IsDeloadWeek && lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) == Today.AddDays(-1 * (int)Today.DayOfWeek))
                // Or the last deload/oldest newsletter was 1+ months ago
                || lastDeload.Date.AddMonths(1) < Today
            );

        return needsDeload;
    }

    /// <summary>
    /// Creates a new instance of the newsletter and saves it.
    /// </summary>
    private async Task<Newsletter> CreateAndAddNewsletterToContext(User user, NewsletterRotation newsletterRotation, bool needsDeload)
    {
        var newsletter = new Newsletter(Today, user, newsletterRotation, needsDeload);
        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();
        return newsletter;
    }

    /// <summary>
    /// User is receiving a new newsletter. Generate a new token for links.
    /// </summary>
    private async Task<string> SetAndSaveNewAuthToken(User user)
    {
        var token = new UserToken(user.Id) 
        {
            // Unsubscribe links need to work for at least 60 days per law
            Expires = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(3) 
        };
        user.UserTokens.Add(token);
        await _context.SaveChangesAsync();
        return token.Token;
    }

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseViewModel>> GetDebugExercises(User user, string token, int count = 1)
    {
        var baseQuery = _context.ExerciseVariations
            .AsNoTracking()
            .Include(v => v.Exercise)
                .ThenInclude(e => e.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteExercise)
            .Include(ev => ev.Variation)
                .ThenInclude(i => i.Intensities)
            .Include(v => v.Variation)
                .ThenInclude(i => i.EquipmentGroups)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Select(a => new
            {
                ExerciseVariation = a,
                a.Variation,
                a.Exercise,
                UserExercise = a.Exercise.UserExercises.FirstOrDefault(uv => uv.User == user),
                UserExerciseVariation = a.UserExerciseVariations.FirstOrDefault(uv => uv.User == user),
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.User == user)
            });

        return (await baseQuery.ToListAsync())
            .GroupBy(i => new { i.Exercise.Id, LastSeen = i.UserExercise?.LastSeen ?? DateOnly.MinValue })
            .OrderBy(a => a.Key.LastSeen)
                .ThenBy(a => Guid.NewGuid())
            .Take(count)
            .SelectMany(e => e)
            .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
            .Select(r => new ExerciseViewModel(user, r.Exercise, r.Variation, r.ExerciseVariation, 
                r.UserExercise, r.UserExerciseVariation, r.UserVariation, intensityLevel: null, Theme: ExerciseTheme.Other, token: token))
            .ToList();
    }

    private async Task UpdateLastSeenDate(User user, IEnumerable<ExerciseViewModel> exercises)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            var userExercise = exerciseDict[exercise].UserExercise;
            if (userExercise != null)
            {
                userExercise.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExercises.Update(userExercise);
            }
            else
            {
                exerciseDict[exercise].UserExercise = userExercise = new UserExercise()
                {
                    ExerciseId = exercise.Id,
                    UserId = user.Id,
                    Progression = UserExercise.MinUserProgression,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExercises.Add(userExercise);
            }
        }

        var exerciseVariationDict = exercises.DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            var userExerciseVariation = exerciseVariationDict[exerciseVariation].UserExerciseVariation;
            if (userExerciseVariation != null)
            {
                userExerciseVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExerciseVariations.Update(userExerciseVariation);
            }
            else
            {
                exerciseVariationDict[exerciseVariation].UserExerciseVariation = userExerciseVariation = new UserExerciseVariation()
                {
                    ExerciseVariationId = exerciseVariation.Id,
                    UserId = user.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExerciseVariations.Add(userExerciseVariation);
            }
        }

        var variationDict = exercises.DistinctBy(e => e.Variation).ToDictionary(e => e.Variation);
        foreach (var variation in variationDict.Keys)
        {
            var userVariation = variationDict[variation].UserVariation;
            if (userVariation != null)
            {
                userVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserVariations.Update(userVariation);
            }
            else
            {
                variationDict[variation].UserVariation = userVariation = new UserVariation()
                {
                    VariationId = variation.Id,
                    UserId = user.Id,
                    LastSeen = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserVariations.Add(userVariation);
            }
        }

        foreach (var item in exercises)
        {
            // Update all the ExerciseViewModels in case we created a new userX record
            item.UserExercise = exerciseDict[item.Exercise].UserExercise;
            item.UserExerciseVariation = exerciseDict[item.Exercise].UserExerciseVariation;
            item.UserVariation = exerciseDict[item.Exercise].UserVariation;
        }

        await scopedCoreContext.SaveChangesAsync();
    }

    #endregion

    [Route("newsletter/{email}")]
    public async Task<IActionResult> Newsletter(string email, string token)
    {
        var user = await GetUser(email, token);
        if (user.Disabled || user.RestDays.HasFlag(RestDaysExtensions.FromDate(Today)))
        {
            return NoContent();
        }

        var previousNewsletter = await GetPreviousNewsletter(user);

        // User has received an email with a confirmation message, but they did not click to confirm their account
        if (previousNewsletter != null && user.LastActive == null)
        {
            return NoContent();
        }

        token = await SetAndSaveNewAuthToken(user);

        var todaysNewsletterRotation = GetTodaysNewsletterRotation(user, previousNewsletter);
        var needsDeload = await CheckNewsletterDeloadStatus(user);

        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload);

        var mainExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(todaysNewsletterRotation.ExerciseType)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
            .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithPrefersWeights(user.PrefersWeights ? true : null)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .CapAtProficiency(needsDeload)
            .WithAtLeastXUniqueMusclesPerExercise(todaysNewsletterRotation.MuscleGroups == MuscleGroups.All ? 3 : 2)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main, token))
            .ToList();

        var warmupExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups/*.UnsetFlag32(warmupCardio.Aggregate((MuscleGroups)0, (acc, next) => acc | next.Exercise.PrimaryMuscles))*/)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .WithAtLeastXUniqueMusclesPerExercise(3)
            .CapAtProficiency(true)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Warmup, token))
            .ToList();

        var warmupCardio = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.Cardio)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .CapAtProficiency(true)
            .Take(2)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Warmup, token))
            .ToList();

        // Recovery exercises
        IList<ExerciseViewModel>? recoveryExercises = null;
        if (user.RecoveryMuscle != MuscleGroups.None)
        {
            // Should recoveru exercises target muscles in isolation?
            recoveryExercises = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.WarmupCooldown)
                .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithSportsFocus(SportsFocus.None)
                .WithPrefersWeights(false)
                .CapAtProficiency(true)
                .Take(1)
                .Query())
                .Select(r => new ExerciseViewModel(r, ExerciseTheme.Warmup, token))
                .Concat((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithExerciseType(ExerciseType.Strength | ExerciseType.Stability | ExerciseType.Cardio)
                    .WithIntensityLevel(IntensityLevel.Recovery)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithSportsFocus(SportsFocus.None)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(user.PrefersWeights ? true : null)
                    .Take(1)
                    .Query())
                    .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main, token)))
                .Concat((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithExerciseType(ExerciseType.Strength | ExerciseType.WarmupCooldown)
                    .WithIntensityLevel(IntensityLevel.Recovery)
                    .WithMuscleContractions(MuscleContractions.Static)
                    .WithSportsFocus(SportsFocus.None)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(false)
                    .CapAtProficiency(true)
                    .Take(1)
                    .Query())
                    .Select(r => new ExerciseViewModel(r, ExerciseTheme.Cooldown, token)))
                .ToList();
        }

        // Sports exercises
        IList<ExerciseViewModel>? sportsExercises = null;
        if (user.SportsFocus != SportsFocus.None)
        {
            sportsExercises = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(todaysNewsletterRotation.ExerciseType)
                .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithSportsFocus(user.SportsFocus)
                .WithRecoveryMuscle(MuscleGroups.None)
                .WithExcludeMuscle(user.RecoveryMuscle)
                .CapAtProficiency(needsDeload)
                .Take(2)
                .Query())
                .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main, token))
                .ToList();
        }

        var cooldownExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .WithAtLeastXUniqueMusclesPerExercise(3)
            .CapAtProficiency(true)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Cooldown, token))
            .ToList();

        IList<ExerciseViewModel>? debugExercises = null;
        if (user.Email == Entities.User.User.DebugUser)
        {
            user.EmailVerbosity = Verbosity.Debug;
            debugExercises = await GetDebugExercises(user, token, count: 3);
            warmupExercises.RemoveAll(_ => true);
            warmupCardio.RemoveAll(_ => true);
            mainExercises.RemoveAll(_ => true);
            cooldownExercises.RemoveAll(_ => true);
        }

        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var viewModel = new NewsletterViewModel(mainExercises, user, newsletter, token)
        {
            AllEquipment = equipmentViewModel,
            SportsExercises = sportsExercises,
            RecoveryExercises = recoveryExercises,
            CooldownExercises = cooldownExercises,
            WarmupExercises = warmupExercises,
            WarmupCardioExercises = warmupCardio,
            DebugExercises = debugExercises
        };

        await UpdateLastSeenDate(user, viewModel.AllExercises);

        return View(nameof(Newsletter), viewModel);
    }
}
