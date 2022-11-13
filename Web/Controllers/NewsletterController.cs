using Web.Data;
using Web.Entities.Exercise;
using Web.Entities.Newsletter;
using Web.Entities.User;
using Web.Extensions;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Web.Controllers;

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
        var weeklyRotation = new NewsletterTypeGroups(user.StrengtheningPreference, user.Frequency);
        var todaysNewsletterRotation = weeklyRotation.First(); // Have to start somewhere
        
        if (previousNewsletter != null)
        {
            todaysNewsletterRotation = weeklyRotation
                // Use Ids to compare so that a minor change to the muscle groups or movement pattern does not reset the weekly rotation
                .SkipWhile(r => r.Id != previousNewsletter.NewsletterRotation.Id)
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
    private async Task<Newsletter> CreateAndAddNewsletterToContext(User user, NewsletterRotation newsletterRotation, bool needsDeload, IList<ExerciseViewModel> actualWorkout)
    {
        var newsletter = new Newsletter(Today, user, newsletterRotation, needsDeload);
        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();

        foreach (var variation in actualWorkout)
        {
            _context.NewsletterVariations.Add(new NewsletterVariation(newsletter, variation.Variation));
        }
        await _context.SaveChangesAsync();

        return newsletter;
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
                r.UserExercise, r.UserExerciseVariation, r.UserVariation, intensityLevel: null, Theme: ExerciseTheme.Extra, token: token))
            .ToList();
    }

    private async Task UpdateLastSeenDate(User user, IEnumerable<ExerciseViewModel> exercises, bool noLog = false)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            var userExercise = exerciseDict[exercise].UserExercise;
            if (userExercise != null && !noLog)
            {
                userExercise.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExercises.Update(userExercise);
            }
            else if (userExercise == null)
            {
                exerciseDict[exercise].UserExercise = userExercise = new UserExercise()
                {
                    ExerciseId = exercise.Id,
                    UserId = user.Id,
                    Progression = UserExercise.MinUserProgression,
                    LastSeen = noLog ? DateOnly.MinValue : DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExercises.Add(userExercise);
            }
        }

        var exerciseVariationDict = exercises.DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            var userExerciseVariation = exerciseVariationDict[exerciseVariation].UserExerciseVariation;
            if (userExerciseVariation != null && !noLog)
            {
                userExerciseVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExerciseVariations.Update(userExerciseVariation);
            }
            else if (userExerciseVariation == null)
            {
                exerciseVariationDict[exerciseVariation].UserExerciseVariation = userExerciseVariation = new UserExerciseVariation()
                {
                    ExerciseVariationId = exerciseVariation.Id,
                    UserId = user.Id,
                    LastSeen = noLog ? DateOnly.MinValue : DateOnly.FromDateTime(DateTime.UtcNow)
                };

                scopedCoreContext.UserExerciseVariations.Add(userExerciseVariation);
            }
        }

        var variationDict = exercises.DistinctBy(e => e.Variation).ToDictionary(e => e.Variation);
        foreach (var variation in variationDict.Keys)
        {
            var userVariation = variationDict[variation].UserVariation;
            if (userVariation != null && !noLog)
            {
                userVariation.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserVariations.Update(userVariation);
            }
            else if (userVariation == null)
            {
                variationDict[variation].UserVariation = userVariation = new UserVariation()
                {
                    VariationId = variation.Id,
                    UserId = user.Id,
                    LastSeen = noLog ? DateOnly.MinValue : DateOnly.FromDateTime(DateTime.UtcNow)
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

        var todaysNewsletterRotation = GetTodaysNewsletterRotation(user, previousNewsletter);
        var needsDeload = await CheckNewsletterDeloadStatus(user);

        var extraExercises = new List<ExerciseViewModel>();
        var mainExercises = new List<ExerciseViewModel>();

        mainExercises.AddRange((await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.Main)
            .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
            .WithMuscleGroups(MuscleGroups.All)
            .WithIncludeMuscle(MuscleGroups.All)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithMuscleMovementPatterns(todaysNewsletterRotation.MovementPatterns, x => 
            {
                x.UniqueMovementPattern = true;
            })
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithPrefersWeights(true)
            .WithIncludeBonus(false)
            .CapAtProficiency(needsDeload)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.None)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main, token))
        );

        // Split up upper and lower body exercise generation so there's a better chance of working an even number of each
        if (todaysNewsletterRotation.MuscleGroups.HasFlag(MuscleGroups.LowerBody))
        {
            var lowerMain = new List<ExerciseViewModel>();
            var lowerBodyFull = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.Main)
                .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
                .IsUnilateral(todaysNewsletterRotation.NewsletterType == NewsletterType.Stability ? true : null)
                .WithExcludeExercises(extraExercises.Select(e => e.Exercise.Id).Concat(mainExercises.Select(e => e.Exercise.Id)))
                .WithAlreadyWorkedMuscles(mainExercises.WorkedMuscles())
                .WithMuscleGroups(MuscleGroups.LowerBody)
                .WithMuscleMovementPatterns(MovementPattern.None)
                .WithExcludeMuscle(user.RecoveryMuscle)
                .WithSportsFocus(SportsFocus.None)
                .WithRecoveryMuscle(MuscleGroups.None)
                .WithPrefersWeights(user.PrefersWeights ? true : null)
                .WithIncludeBonus(user.IncludeBonus ? null : false)
                .CapAtProficiency(needsDeload)
                .WithAtLeastXUniqueMusclesPerExercise(todaysNewsletterRotation.MuscleGroups == MuscleGroups.All ? 3 : 2)
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles, skip: 1)
                .Query());

            foreach (var exercise in lowerBodyFull)
            {
                var primaryMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => lowerMain.Sum(r => r.Variation.PrimaryMuscles.HasFlag(v) ? 1 : 0));
                var allMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => lowerMain.Sum(r => r.Variation.AllMuscles.HasFlag(v) ? 1 : 0));
                var firstGoAround = BitOperations.PopCount((ulong)MuscleGroups.LowerBody.UnsetFlag32(exercise.Variation.PrimaryMuscles.UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate(mainExercises.WorkedMuscles(), (curr, n) => curr | n.Key)))) <= (BitOperations.PopCount((ulong)MuscleGroups.LowerBody) - 1);
                if (firstGoAround)
                {
                    lowerMain.Add(new ExerciseViewModel(exercise, ExerciseTheme.Main, token));
                }
                else
                {
                    extraExercises.Add(new ExerciseViewModel(exercise, ExerciseTheme.Extra, token));
                }
            }

            mainExercises.AddRange(lowerMain.OrderByDescending(vm => BitOperations.PopCount((ulong)vm.Variation.PrimaryMuscles)));
        }

        if (todaysNewsletterRotation.MuscleGroups.HasFlag(MuscleGroups.UpperBody))
        {
            var upperMain = new List<ExerciseViewModel>();
            var upperBodyFull = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.Main)
                .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
                .IsUnilateral(todaysNewsletterRotation.NewsletterType == NewsletterType.Stability ? true : null)
                .WithExcludeExercises(extraExercises.Select(e => e.Exercise.Id).Concat(mainExercises.Select(e => e.Exercise.Id)))
                .WithAlreadyWorkedMuscles(mainExercises.WorkedMuscles())
                .WithMuscleGroups(MuscleGroups.UpperBody)
                .WithMuscleMovementPatterns(MovementPattern.None)
                .WithExcludeMuscle(user.RecoveryMuscle)
                .WithSportsFocus(SportsFocus.None)
                .WithRecoveryMuscle(MuscleGroups.None)
                .WithPrefersWeights(user.PrefersWeights ? true : null)
                .WithIncludeBonus(user.IncludeBonus ? null : false)
                .CapAtProficiency(needsDeload)
                .WithAtLeastXUniqueMusclesPerExercise(todaysNewsletterRotation.MuscleGroups == MuscleGroups.All ? 3 : 2)
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles, skip: 1)
                .Query());

            foreach (var exercise in upperBodyFull)
            {
                var primaryMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => upperMain.Sum(r => r.Variation.PrimaryMuscles.HasFlag(v) ? 1 : 0));
                var allMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => upperMain.Sum(r => r.Variation.AllMuscles.HasFlag(v) ? 1 : 0));
                var firstGoAround = BitOperations.PopCount((ulong)MuscleGroups.UpperBody.UnsetFlag32(exercise.Variation.PrimaryMuscles.UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate(mainExercises.WorkedMuscles(), (curr, n) => curr | n.Key)))) <= (BitOperations.PopCount((ulong)MuscleGroups.UpperBody) - 1);
                if (firstGoAround)
                {
                    upperMain.Add(new ExerciseViewModel(exercise, ExerciseTheme.Main, token));
                }
                else
                {
                    extraExercises.Add(new ExerciseViewModel(exercise, ExerciseTheme.Extra, token));
                }
            }

            mainExercises.AddRange(upperMain.OrderByDescending(vm => BitOperations.PopCount((ulong)vm.Variation.PrimaryMuscles)));
        }

        var coreBodyFull = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.Main)
                .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
                .IsUnilateral(todaysNewsletterRotation.NewsletterType == NewsletterType.Stability ? true : null)
                .WithExcludeExercises(extraExercises.Select(e => e.Exercise.Id).Concat(mainExercises.Select(e => e.Exercise.Id)))
                .WithMuscleGroups(MuscleGroups.Core)
                .WithIncludeMuscle(MuscleGroups.Core)
                .WithExcludeMuscle(user.RecoveryMuscle)
                .WithSportsFocus(SportsFocus.None)
                .WithRecoveryMuscle(MuscleGroups.None)
                .WithPrefersWeights(user.PrefersWeights ? true : null)
                .WithIncludeBonus(user.IncludeBonus ? null : false)
                .CapAtProficiency(needsDeload)
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.None)
                //.WithAtLeastXUniqueMusclesPerExercise(1)
                .Query())
                // Need someway to say kettlebell swings and deadlifts are not core workouts
                .Where(vm => BitOperations.PopCount((ulong)vm.Variation.PrimaryMuscles.UnsetFlag32(MuscleGroups.Core)) <= 2)
                .Take(2)
                .ToList();

        if (coreBodyFull.Any())
        {
            mainExercises.Add(new ExerciseViewModel(coreBodyFull.First(), ExerciseTheme.Main, token));
        }
        if (coreBodyFull.Count > 1)
        {
            extraExercises.Add(new ExerciseViewModel(coreBodyFull.Last(), ExerciseTheme.Extra, token));
        }

        var warmupMovement = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups/*.UnsetFlag32(warmupCardio.Aggregate((MuscleGroups)0, (acc, next) => acc | next.Exercise.PrimaryMuscles))*/)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .WithMuscleMovementPatterns(todaysNewsletterRotation.MovementPatterns, x => 
            {
                x.UniqueMovementPattern = true;
            })
            .CapAtProficiency(true)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Warmup, token))
            .ToList();

        var warmupExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups/*.UnsetFlag32(warmupCardio.Aggregate((MuscleGroups)0, (acc, next) => acc | next.Exercise.PrimaryMuscles))*/)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithAlreadyWorkedMuscles(warmupMovement.WorkedMuscles())
            .WithExcludeExercises(warmupMovement.Select(e => e.Exercise.Id))
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovementPatterns(MovementPattern.None)
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

        // Get the heart rate up. Can work any muscle.
        // Ideal is 2-5 minutes. We want to provide at least 2x60s exercises.
        var warmupCardio = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Pylometric)
            .WithMuscleGroups(MuscleGroups.All)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .CapAtProficiency(true)
            .WithAtLeastXUniqueMusclesPerExercise(3)
            .Query())
            .Take(2)
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Warmup, token))
            .ToList();

        // Recovery exercises
        IList<ExerciseViewModel>? recoveryExercises = null;
        if (user.RecoveryMuscle != MuscleGroups.None)
        {
            // Should recovery exercises target muscles in isolation?
            recoveryExercises = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.WarmupCooldown)
                .WithIntensityLevel(IntensityLevel.WarmupCooldown)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithSportsFocus(SportsFocus.None)
                .WithPrefersWeights(false)
                .CapAtProficiency(true)
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, ExerciseTheme.Warmup, token))
                .Concat((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithExerciseType(ExerciseType.Main)
                    .WithIntensityLevel(IntensityLevel.Recovery)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithSportsFocus(SportsFocus.None)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(user.PrefersWeights ? true : null)
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main, token)))
                .Concat((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithExerciseType(ExerciseType.WarmupCooldown)
                    .WithIntensityLevel(IntensityLevel.Recovery)
                    .WithMuscleContractions(MuscleContractions.Static)
                    .WithSportsFocus(SportsFocus.None)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(false)
                    .CapAtProficiency(true)
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, ExerciseTheme.Cooldown, token)))
                .ToList();
        }

        // Sports exercises
        IList<ExerciseViewModel>? sportsExercises = null;
        if (user.SportsFocus != SportsFocus.None)
        {
            sportsExercises = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.Main)
                .WithIntensityLevel(todaysNewsletterRotation.IntensityLevel)
                .IsUnilateral(todaysNewsletterRotation.NewsletterType == NewsletterType.Stability ? true : null)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
                .WithIncludeMuscle(todaysNewsletterRotation.MuscleGroups)
                .WithExcludeMuscle(user.RecoveryMuscle)
                .WithSportsFocus(user.SportsFocus)
                .WithRecoveryMuscle(MuscleGroups.None)
                .CapAtProficiency(needsDeload)
                .Query())
                .Take(2)
                .Select(r => new ExerciseViewModel(r, ExerciseTheme.Other, token))
                .ToList();
        }

        var cooldownExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithIntensityLevel(IntensityLevel.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups)
            .WithExcludeMuscle(user.RecoveryMuscle)
            .WithRecoveryMuscle(MuscleGroups.None)
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
            extraExercises.RemoveAll(_ => true);
            mainExercises.RemoveAll(_ => true);
            cooldownExercises.RemoveAll(_ => true);
        }

        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload, mainExercises);
        var viewModel = new NewsletterViewModel(user, newsletter, token)
        {
            ExtraExercises = extraExercises,
            MainExercises = mainExercises,
            AllEquipment = equipmentViewModel,
            SportsExercises = sportsExercises,
            RecoveryExercises = recoveryExercises,
            CooldownExercises = cooldownExercises,
            WarmupExercises = warmupExercises.Concat(warmupMovement).Concat(warmupCardio).ToList(),
            DebugExercises = debugExercises
        };

        await UpdateLastSeenDate(user, viewModel.AllExercises);
        await UpdateLastSeenDate(user, viewModel.ExtraExercises, noLog: true);

        return View(nameof(Newsletter), viewModel);
    }
}
