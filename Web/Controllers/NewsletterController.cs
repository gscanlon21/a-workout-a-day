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
using Web.Data.QueryBuilder;

namespace Web.Controllers;

public class NewsletterController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Newsletter";

    /// <summary>
    /// ViewData key for whether this is a deload week.
    /// </summary>
    public const string ViewData_Deload = "Deload";

    /// <summary>
    /// How much to scale the user's proficiency by during a deload week.
    /// 
    /// Using .05 over 50% because the max progression is 95 and we don't want to drop the user back below 50.
    /// </summary>
    private const double DeloadWeekIntensityModifier = 0.55;

    /// <summary>
    /// Today's date from UTC.
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
        // PERF: Run this as a split query when EF Core supports batching those
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
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    internal async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user)
    {
        var weeklyRotation = new NewsletterTypeGroups(user.StrengtheningPreference, user.Frequency);
        var todaysNewsletterRotation = weeklyRotation.First(); // Have to start somewhere
        
        var previousNewsletter = await _context.Newsletters
            .Where(n => n.User == user)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == user.Frequency)
            .OrderBy(n => n.Date)
            .ThenBy(n => n.Id) // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            .LastOrDefaultAsync();

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
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    internal async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.Newsletters
            .Where(n => n.User == user)
            .OrderBy(n => n.Date)
            .LastOrDefaultAsync(n => n.IsDeloadWeek);

        bool needsDeload = 
            // Dates are the same week. Keep the deload going until the week is over.
            (lastDeload != null && lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) == Today.AddDays(-1 * (int)Today.DayOfWeek))
            // Or the last deload was 1+ months ago.
            || (lastDeload != null && lastDeload.Date < Today.AddDays(-7 * user.DeloadAfterEveryXWeeks))
            // Or there has never been a deload before, look at the user's created date.
            || (lastDeload == null && user.CreatedDate < Today.AddDays(-7 * user.DeloadAfterEveryXWeeks));

        TimeSpan timeUntilDeload = (needsDeload, lastDeload) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(user.CreatedDate.DayNumber - Today.AddDays(-7 * user.DeloadAfterEveryXWeeks).DayNumber),
            // Calculate the next deload date using the last deload's date.
            (false, not null) => TimeSpan.FromDays(lastDeload.Date.DayNumber - Today.AddDays(-7 * user.DeloadAfterEveryXWeeks).DayNumber),
            _ => TimeSpan.Zero
        };

        return (needsDeload, timeUntilDeload);
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
                r.UserExercise, r.UserExerciseVariation, r.UserVariation, 
                easierVariation: null, harderVariation: null, 
                intensityLevel: null, Theme: ExerciseTheme.Extra, token: token))
            .ToList();
    }

    private async Task UpdateLastSeenDate(User user, IEnumerable<ExerciseViewModel> exercises, IEnumerable<ExerciseViewModel> noLog)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.Concat(noLog).DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            if (exerciseDict[exercise].UserExercise != null && exercises.Select(vm => vm.Exercise).Contains(exercise))
            {
                exerciseDict[exercise].UserExercise!.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExercises.Update(exerciseDict[exercise].UserExercise!);
            }
            else if (exerciseDict[exercise].UserExercise == null)
            {
                exerciseDict[exercise].UserExercise = new UserExercise()
                {
                    ExerciseId = exercise.Id,
                    UserId = user.Id,
                    LastSeen = exercises.Select(vm => vm.Exercise).Contains(exercise) ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.MinValue
                };

                scopedCoreContext.UserExercises.Add(exerciseDict[exercise].UserExercise!);
            }
        }

        var exerciseVariationDict = exercises.Concat(noLog).DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            if (exerciseVariationDict[exerciseVariation].UserExerciseVariation != null && exercises.Select(vm => vm.ExerciseVariation).Contains(exerciseVariation))
            {
                exerciseVariationDict[exerciseVariation].UserExerciseVariation!.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserExerciseVariations.Update(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
            else if (exerciseVariationDict[exerciseVariation].UserExerciseVariation == null)
            {
                exerciseVariationDict[exerciseVariation].UserExerciseVariation = new UserExerciseVariation()
                {
                    ExerciseVariationId = exerciseVariation.Id,
                    UserId = user.Id,
                    LastSeen = exercises.Select(vm => vm.ExerciseVariation).Contains(exerciseVariation) ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.MinValue
                };

                scopedCoreContext.UserExerciseVariations.Add(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
        }

        var variationDict = exercises.Concat(noLog).DistinctBy(e => e.Variation).ToDictionary(e => e.Variation);
        foreach (var variation in variationDict.Keys)
        {
            if (variationDict[variation].UserVariation != null && exercises.Select(vm => vm.Variation).Contains(variation))
            {
                variationDict[variation].UserVariation!.LastSeen = DateOnly.FromDateTime(DateTime.UtcNow);
                scopedCoreContext.UserVariations.Update(variationDict[variation].UserVariation!);
            }
            else if (variationDict[variation].UserVariation == null)
            {
                variationDict[variation].UserVariation = new UserVariation()
                {
                    VariationId = variation.Id,
                    UserId = user.Id,
                    LastSeen = exercises.Select(vm => vm.Variation).Contains(variation) ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.MinValue
                };

                scopedCoreContext.UserVariations.Add(variationDict[variation].UserVariation!);
            }
        }

        foreach (var item in exercises.Concat(noLog))
        {
            // Update all the ExerciseViewModels in case we created a new userX record
            item.UserExercise = exerciseDict[item.Exercise].UserExercise;
            item.UserExerciseVariation = exerciseVariationDict[item.ExerciseVariation].UserExerciseVariation;
            item.UserVariation = variationDict[item.Variation].UserVariation;
        }

        await scopedCoreContext.SaveChangesAsync();
    }

    internal async Task<List<ExerciseViewModel>> GetWarmupExercises(User user, NewsletterRotation todaysNewsletterRotation, string token)
    {
        var warmupMovement = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            // FIXME? If a hip hinge warmup doesn't use any of the muscles in the target set, do we still show it as a warmup?
            // As of 2022-11-26, we do not.
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                // Choose movement warmups that also target at least 1 stretch or strength muscle in today's target muscle set
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .WithMovementPatterns(todaysNewsletterRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x => {
                x.AllowLesserProgressions = false;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.WarmupCooldown, ExerciseTheme.Warmup, token))
            .ToList();

        var warmupExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            .WithProficency(x => {
                x.AllowLesserProgressions = false;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithAlreadyWorkedMuscles(warmupMovement.WorkedMuscles(muscleTarget: vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles))
            .WithExcludeExercises(warmupMovement.Select(e => e.Exercise.Id))
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMovementPatterns(MovementPattern.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            //.WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles, skip: 1)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.WarmupCooldown, ExerciseTheme.Warmup, token))
            .ToList();

        // Get the heart rate up. Can work any muscle.
        // Ideal is 2-5 minutes. We want to provide at least 2x60s exercises.
        var warmupCardio = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            // We just want to get the blood flowing. It doesn't matter what muscles these work.
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                // Work unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.StabilityMuscles;
            })
            .WithProficency(x => {
                x.AllowLesserProgressions = false;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Pylometric)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .Build()
            .Query())
            .Take(2)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.WarmupCooldown, ExerciseTheme.Warmup, token))
            .ToList();

        return warmupExercises.Concat(warmupMovement).Concat(warmupCardio).ToList();
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

        // User was already sent a newsletter today
        if (!user.Email.EndsWith("finerfettle.com") // Allow test users to see multiple emails per day
            && await _context.Newsletters.Where(n => n.User == user).AnyAsync(n => n.Date == Today))
        {
            return NoContent();
        }

        // User has received an email with a confirmation message, but they did not click to confirm their account
        if (await _context.Newsletters.AnyAsync(n => n.User == user) && user.LastActive == null)
        {
            return NoContent();
        }

        var todaysNewsletterRotation = await GetTodaysNewsletterRotation(user);
        var needsDeload = await CheckNewsletterDeloadStatus(user);
        var todaysMainIntensityLevel = needsDeload.needsDeload ? IntensityLevel.Maintain : todaysNewsletterRotation.IntensityLevel;

        var warmupExercises = await GetWarmupExercises(user, todaysNewsletterRotation, token);

        var mainExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
            })
            .WithMovementPatterns(todaysNewsletterRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
                x.CapAtUsersProficiencyPercent = needsDeload.needsDeload ? DeloadWeekIntensityModifier : null;
            })
            .WithExerciseType(ExerciseType.Main)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithIncludeBonus(false)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.MuscleTarget)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Main, token))
            .ToList();

        var extraExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
                x.CapAtUsersProficiencyPercent = needsDeload.needsDeload ? DeloadWeekIntensityModifier : null;
            })
            .WithExcludeExercises(warmupExercises.Select(vm => vm.Exercise.Id))
            .WithExerciseType(ExerciseType.Main)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            // Start off with some vigor 
            .WithMuscleMovement(MuscleMovement.Pylometric)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Endurance, ExerciseTheme.Extra, token))
            .ToList();

        var otherFull = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = todaysNewsletterRotation.MuscleGroups == MuscleGroups.All ? 3 : 2;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
                x.CapAtUsersProficiencyPercent = needsDeload.needsDeload ? DeloadWeekIntensityModifier : null;
            })
            .WithExerciseType(ExerciseType.Main)
            .IsUnilateral(null)
            .WithExcludeExercises(extraExercises.Select(e => e.Exercise.Id).Concat(mainExercises.Select(e => e.Exercise.Id)))
            //.WithAlreadyWorkedMuscles(mainExercises.WorkedMuscles()) We want all muscles included so we have something for the adjunct section
            // Leave movement patterns to the first part of the main section - so we don't work a pull on a push day.
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles, skip: 1)
            .Build()
            .Query());

        var otherMain = new List<ExerciseViewModel>();
        foreach (var exercise in otherFull)
        {
            var musclesWorkedSoFar = otherMain.WorkedMuscles(vm => vm.Variation.StrengthMuscles, addition: mainExercises.WorkedMuscles(vm => vm.Variation.StrengthMuscles));
            var hasUnworkedMuscleGroup = (exercise.Variation.StrengthMuscles & todaysNewsletterRotation.MuscleGroups).UnsetFlag32(musclesWorkedSoFar & todaysNewsletterRotation.MuscleGroups) > MuscleGroups.None;
            if (hasUnworkedMuscleGroup)
            {
                otherMain.Add(new ExerciseViewModel(exercise, todaysMainIntensityLevel, ExerciseTheme.Main, token));
            }
            else
            {
                extraExercises.Add(new ExerciseViewModel(exercise, IntensityLevel.Endurance, ExerciseTheme.Extra, token));
            }
        }

        if (!user.IsNewToFitness)
        {
            mainExercises.AddRange(otherMain.OrderByDescending(vm => BitOperations.PopCount((ulong)vm.Variation.StrengthMuscles)));
        }

        var coreBodyFull = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.Core, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                // Not null so we at least choose unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
                x.CapAtUsersProficiencyPercent = needsDeload.needsDeload ? DeloadWeekIntensityModifier : null;
            })
            .WithExerciseType(ExerciseType.Main)
            .IsUnilateral(null)
            .WithExcludeExercises(extraExercises.Select(e => e.Exercise.Id).Concat(mainExercises.Select(e => e.Exercise.Id)))
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithPrefersWeights(null)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.None)
            .Build()
            .Query())
            .Take(2)
            .ToList();

        if (coreBodyFull.Any())
        {
            mainExercises.Add(new ExerciseViewModel(coreBodyFull.First(), todaysMainIntensityLevel, ExerciseTheme.Main, token));
        }
        if (coreBodyFull.Count > 1)
        {
            extraExercises.Add(new ExerciseViewModel(coreBodyFull.Last(), IntensityLevel.Endurance, ExerciseTheme.Extra, token));
        }

        // Recovery exercises
        IList<ExerciseViewModel>? recoveryExercises = null;
        if (user.RecoveryMuscle != MuscleGroups.None)
        {
            // Should recovery exercises target muscles in isolation?
            recoveryExercises = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithProficency(x => {
                    x.DoCapAtProficiency = true;
                })
                .WithExerciseType(ExerciseType.WarmupCooldown)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .WithSportsFocus(SportsFocus.None)
                .WithPrefersWeights(false)
                .Build()
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, IntensityLevel.WarmupCooldown, ExerciseTheme.Warmup, token))
                .Concat((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithExerciseType(ExerciseType.Main)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithSportsFocus(SportsFocus.None)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Main, token)))
                .Concat((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithProficency(x => {
                        x.DoCapAtProficiency = true;
                    })
                    .WithExerciseType(ExerciseType.WarmupCooldown)
                    .WithMuscleContractions(MuscleContractions.Static)
                    .WithSportsFocus(SportsFocus.None)
                    .WithRecoveryMuscle(user.RecoveryMuscle)
                    .WithPrefersWeights(false)
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Cooldown, token)))
                .ToList();
        }

        // Sports exercises
        IList<ExerciseViewModel>? sportsExercises = null;
        if (user.SportsFocus != SportsFocus.None)
        {
            sportsExercises = (await new ExerciseQueryBuilder(_context)
                .WithUser(user)
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x => {
                    x.ExcludeMuscleGroups = user.RecoveryMuscle;
                })
                .WithProficency(x => {
                    x.DoCapAtProficiency = needsDeload.needsDeload;
                })
                .WithExerciseType(ExerciseType.Main)
                .IsUnilateral(null)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithSportsFocus(user.SportsFocus)
                .WithRecoveryMuscle(MuscleGroups.None)
                .Build()
                .Query())
                .Take(2)
                .Select(r => new ExerciseViewModel(r, todaysNewsletterRotation.IntensityLevel, ExerciseTheme.Other, token))
                .ToList();
        }

        var cooldownExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            .WithProficency(x => {
                x.AllowLesserProgressions = false;
            })
            // sa. Bar hang is both a strength exercise and a cooldown stretch...
            .WithExcludeExercises(mainExercises.Concat(extraExercises).Select(vm => vm.Exercise.Id))
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithPrefersWeights(false)
            .WithIncludeBonus(user.IncludeBonus ? null : false)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.WarmupCooldown, ExerciseTheme.Cooldown, token))
            .ToList();

        IList<ExerciseViewModel>? debugExercises = null;
        if (user.Email == Entities.User.User.DebugUser)
        {
            user.EmailVerbosity = Verbosity.Debug;
            debugExercises = await GetDebugExercises(user, token, count: 3);
            warmupExercises.RemoveAll(_ => true);
            extraExercises.RemoveAll(_ => true);
            mainExercises.RemoveAll(_ => true);
            cooldownExercises.RemoveAll(_ => true);
        }

        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload.needsDeload, mainExercises);
        var viewModel = new NewsletterViewModel(user, newsletter, token)
        {
            ExtraExercises = extraExercises,
            MainExercises = mainExercises,
            AllEquipment = equipmentViewModel,
            SportsExercises = sportsExercises,
            RecoveryExercises = recoveryExercises,
            CooldownExercises = cooldownExercises,
            WarmupExercises = warmupExercises,
            DebugExercises = debugExercises,
            TimeUntilDeload = needsDeload.timeUntilDeload
        };

        await UpdateLastSeenDate(user, viewModel.AllExercises, viewModel.ExtraExercises);

        ViewData[ViewData_Deload] = needsDeload.needsDeload;
        return View(nameof(Newsletter), viewModel);
    }
}
