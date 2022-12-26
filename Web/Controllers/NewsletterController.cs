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
using Web.Services;

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
    private readonly UserService _userService;

    public NewsletterController(CoreContext context, UserService userService, IServiceScopeFactory serviceScopeFactory) : base(context)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _userService = userService;
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
            .Include(v => v.Variation)
                .ThenInclude(i => i.EquipmentGroups)
                    .ThenInclude(eg => eg.Children)
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

    #endregion

    #region Warmup

    internal async Task<List<ExerciseViewModel>> GetWarmupExercises(User user, NewsletterRotation todaysNewsletterRotation, string token, 
        IEnumerable<Exercise>? excludeExercises = null, IEnumerable<Variation>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups | MuscleGroups.Core, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = 2;
            })
            .WithProficency(x => {
                x.AllowLesserProgressions = false;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            //.WithAlreadyWorkedMuscles(warmupMovement.WorkedMuscles(muscleTarget: vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles))
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(excludeExercises);
                x.AddExcludeVariations(excludeVariations);
            })
            .WithMuscleContractions(MuscleContractions.Dynamic)
            //.WithMovementPatterns(MovementPattern.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .WithBonus(user.IncludeBonus)
            //.WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles, skip: 1)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
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
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(excludeExercises);
                x.AddExcludeVariations(excludeVariations);
            })
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .WithBonus(user.IncludeBonus)
            .Build()
            .Query())
            .Take(2)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
            .ToList();

        return warmupExercises.Concat(warmupCardio).ToList();
    }

    #endregion

    #region Cooldown

    internal async Task<List<ExerciseViewModel>> GetCooldownExercises(User user, NewsletterRotation todaysNewsletterRotation, string token, 
        IEnumerable<Exercise>? excludeExercises = null, IEnumerable<Variation>? excludeVariations = null)
    {
        return (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups | MuscleGroups.Core, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = 2;
            })
            .WithProficency(x => {
                x.AllowLesserProgressions = false;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(excludeExercises);
                x.AddExcludeVariations(excludeVariations);
            })
            //.WithAlreadyWorkedMuscles(cooldownMovement.WorkedMuscles(muscleTarget: vm => vm.Variation.StretchMuscles))
            //.WithMovementPatterns(MovementPattern.None)
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithOnlyWeights(false)
            .WithBonus(user.IncludeBonus)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Cooldown, ExerciseTheme.Cooldown, token))
            .ToList();
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
        var needsDeload = await _userService.CheckNewsletterDeloadStatus(user);
        var todaysMainIntensityLevel = needsDeload.needsDeload ? IntensityLevel.Maintain : todaysNewsletterRotation.IntensityLevel;

        // Choose cooldown first
        var cooldownExercises = await GetCooldownExercises(user, todaysNewsletterRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysNewsletterRotation, token, /*sa. Cat/Cow*/ excludeVariations: cooldownExercises.Select(vm => vm.Variation));

        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
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
            .WithExcludeExercises(x =>
            {
                // Exclude warmup so we don't get two of something such as Pushup Plus which is both a warmup and main exercise
                x.AddExcludeVariations(warmupExercises.Select(e => e.Variation));
            })
            .WithExerciseType(ExerciseType.Main)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithBonus(user.IncludeBonus)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.MuscleTarget)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Main, token))
            .ToList();

        // Grabs 1 pylometric exercise to start off the adjunct section, in case their heart rate has fallen.
        var extraExercises = (await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeMuscleGroups = user.RecoveryMuscle;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
                x.CapAtUsersProficiencyPercent = needsDeload.needsDeload ? DeloadWeekIntensityModifier : null;
            })
            // Exclude warmup because this is looking for pylometric and we don't want to use something from warmupCardio
            .WithExcludeExercises(x =>
            {
                // sa. exclude the same Mountain Climber variation we worked for a warmup
                x.AddExcludeVariations(warmupExercises.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Main)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            // Start off with some vigor 
            .WithMuscleMovement(MuscleMovement.Pylometric)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithBonus(user.IncludeBonus)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Endurance, ExerciseTheme.Extra, token))
            .ToList();

        // Grabs a full workout of accessory exercises.
        // The ones that work the same muscle groups that the core set
        // ... are moved to the adjunct section in case the user has a little something extra.
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
            .WithExcludeExercises(x =>
            {
                // sa. exclude all Squat variations if we already worked any Squat variation earlier
                x.AddExcludeExercises(mainExercises.Concat(extraExercises).Select(e => e.Exercise));
                // sa. exclude the same Deadbug variation we worked for a warmup
                x.AddExcludeVariations(warmupExercises.Select(e => e.Variation));
                // sa. exclude the same Bar Hang variation we worked for a warmup
                x.AddExcludeVariations(cooldownExercises.Select(e => e.Variation));
            })
            //.WithAlreadyWorkedMuscles(mainExercises.WorkedMuscles()) We want all muscles included so we have something for the adjunct section
            // Leave movement patterns to the first part of the main section - so we don't work a pull on a push day.
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithBonus(user.IncludeBonus)
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

        // User is new to fitness? Don't add additional accessory exercises to the core set.
        if (!user.IsNewToFitness)
        {
            mainExercises.AddRange(otherMain.OrderByDescending(vm => BitOperations.PopCount((ulong)vm.Variation.StrengthMuscles)));
        }

        // Grabs 2 core exercises.
        // One is moved to the end of the main section.
        // One is moved to the end of the adjunct section.
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
            .WithExcludeExercises(x =>
            {
                // sa. exclude all Plank variations if we already worked any Plank variation earlier
                x.AddExcludeExercises(mainExercises.Concat(extraExercises).Select(vm => vm.Exercise));
                // sa. exclude the same Deadbug variation we worked for a warmup
                x.AddExcludeVariations(warmupExercises.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithBonus(user.IncludeBonus)
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
                .WithOnlyWeights(false)
                .Build()
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
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
                    .WithOnlyWeights(false)
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
                .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Other, token))
                .ToList();
        }

        IList<ExerciseViewModel>? debugExercises = null;
        if (user.Email == Entities.User.User.DebugUser)
        {
            user.EmailVerbosity = Verbosity.Debug;
            debugExercises = await GetDebugExercises(user, token, count: 2);
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
