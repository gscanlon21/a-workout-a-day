using Web.Data;
using Web.Entities.Exercise;
using Web.Entities.Newsletter;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using Web.Data.QueryBuilder;
using Web.Services;
using Web.Code.Extensions;

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
        return await _context.Users.AsSplitQuery()
            // For displaying ignored exercises in the bottom of the newsletter
            .Include(u => u.UserExercises)
                .ThenInclude(ue => ue.Exercise)
            .Include(u => u.UserVariations)
                .ThenInclude(uv => uv.Variation)
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
        var weeklyRotation = new NewsletterTypeGroups(user.Frequency);
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
    private async Task<Newsletter> CreateAndAddNewsletterToContext(User user, NewsletterRotation newsletterRotation, bool needsDeload, bool needsFunctionalRefresh, bool needsAccessoryRefresh, IEnumerable<ExerciseViewModel> strengthExercises)
    {
        var newsletter = new Newsletter(Today, user, newsletterRotation, isDeloadWeek: needsDeload, needsFunctionalRefresh: needsFunctionalRefresh, needsAccessoryRefresh: needsAccessoryRefresh);
        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();

        foreach (var variation in strengthExercises)
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
            .Include(ev => ev.Variation)
                .ThenInclude(i => i.DefaultInstruction)
            .Include(v => v.Variation)
                .ThenInclude(i => i.Instructions.Where(eg => eg.Parent == null))
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Include(v => v.Variation)
                .ThenInclude(i => i.Instructions.Where(eg => eg.Parent == null))
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

    /// <summary>
    ///     Updates the last seen date of the exercise by the user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="exercises"></param>
    /// <param name="noLog">
    ///     These get the last seen date logged to yesterday instead of today so that they are still marked seen, 
    ///     but more likely? to make it into the main section next time.
    /// </param>
    private async Task UpdateLastSeenDate(User user, IEnumerable<ExerciseViewModel> exercises, IEnumerable<ExerciseViewModel> noLog)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedCoreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var exerciseDict = exercises.Concat(noLog).DistinctBy(e => e.Exercise).ToDictionary(e => e.Exercise);
        foreach (var exercise in exerciseDict.Keys)
        {
            DateOnly logDate = exercises.Select(vm => vm.Exercise).Contains(exercise) ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            if (exerciseDict[exercise].UserExercise != null)
            {
                exerciseDict[exercise].UserExercise!.LastSeen = logDate;
                scopedCoreContext.UserExercises.Update(exerciseDict[exercise].UserExercise!);
            }
            else if (exerciseDict[exercise].UserExercise == null)
            {
                exerciseDict[exercise].UserExercise = new UserExercise()
                {
                    ExerciseId = exercise.Id,
                    UserId = user.Id,
                    LastSeen = logDate
                };

                scopedCoreContext.UserExercises.Add(exerciseDict[exercise].UserExercise!);
            }
        }

        var exerciseVariationDict = exercises.Concat(noLog).DistinctBy(e => e.ExerciseVariation).ToDictionary(e => e.ExerciseVariation);
        foreach (var exerciseVariation in exerciseVariationDict.Keys)
        {
            DateOnly logDate = exercises.Select(vm => vm.ExerciseVariation).Contains(exerciseVariation) ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            if (exerciseVariationDict[exerciseVariation].UserExerciseVariation != null)
            {
                exerciseVariationDict[exerciseVariation].UserExerciseVariation!.LastSeen = logDate;
                scopedCoreContext.UserExerciseVariations.Update(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
            else if (exerciseVariationDict[exerciseVariation].UserExerciseVariation == null)
            {
                exerciseVariationDict[exerciseVariation].UserExerciseVariation = new UserExerciseVariation()
                {
                    ExerciseVariationId = exerciseVariation.Id,
                    UserId = user.Id,
                    LastSeen = logDate
                };

                scopedCoreContext.UserExerciseVariations.Add(exerciseVariationDict[exerciseVariation].UserExerciseVariation!);
            }
        }

        var variationDict = exercises.Concat(noLog).DistinctBy(e => e.Variation).ToDictionary(e => e.Variation);
        foreach (var variation in variationDict.Keys)
        {
            DateOnly logDate = exercises.Select(vm => vm.Variation).Contains(variation) ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
            if (variationDict[variation].UserVariation != null)
            {
                variationDict[variation].UserVariation!.LastSeen = logDate;
                scopedCoreContext.UserVariations.Update(variationDict[variation].UserVariation!);
            }
            else if (variationDict[variation].UserVariation == null)
            {
                variationDict[variation].UserVariation = new UserVariation()
                {
                    VariationId = variation.Id,
                    UserId = user.Id,
                    LastSeen = logDate
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
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)todaysNewsletterRotation.MuscleGroups) > 10 ? 3 : 2;
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
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
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
            .WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(excludeExercises);
                x.AddExcludeVariations(excludeVariations);
            })
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
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
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)todaysNewsletterRotation.MuscleGroups) > 10 ? 3 : 2;
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
        if (await _context.Newsletters.Where(n => n.User == user).AnyAsync(n => n.Date == Today)
            // Allow test users to see multiple emails per day
            && !user.Email.EndsWith("finerfettle.com"))
        {
            return NoContent();
        }

        // User has received an email with a confirmation message, but they did not click to confirm their account
        if (await _context.Newsletters.AnyAsync(n => n.User == user) && user.LastActive == null)
        {
            return NoContent();
        }

        // Return the debug newsletter for the debug user
        if (user.Email == Entities.User.User.DebugUser)
        {
            return await DebugNewsletter(user, token);
        }

        // The exercise queryer requires UserExercise/UserExerciseVariation/UserVariation records to have already been made
        _context.AddMissing(await _context.UserExercises.Where(ue => ue.User == user).Select(ue => ue.ExerciseId).ToListAsync(), 
            await _context.Exercises.Select(e => new { e.Id, e.Proficiency }).ToListAsync(), k => k.Id, e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserExercise.MinUserProgression : e.Proficiency });

        _context.AddMissing(await _context.UserExerciseVariations.Where(ue => ue.User == user).Select(uev => uev.ExerciseVariationId).ToListAsync(), 
            await _context.ExerciseVariations.Select(ev => ev.Id).ToListAsync(), evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id });

        _context.AddMissing(await _context.UserVariations.Where(ue => ue.User == user).Select(uv => uv.VariationId).ToListAsync(), 
            await _context.Variations.Select(v => v.Id).ToListAsync(), vId => new UserVariation() { VariationId = vId, UserId = user.Id });

        await _context.SaveChangesAsync();

        var todaysNewsletterRotation = await GetTodaysNewsletterRotation(user);
        var needsDeload = await _userService.CheckNewsletterDeloadStatus(user);
        var needsFunctionalRefresh = await _userService.CheckFunctionalRefreshStatus(user);
        var needsAccessoryRefresh = await _userService.CheckAccessoryRefreshStatus(user);
        var todaysMainIntensityLevel = needsDeload.needsDeload ? IntensityLevel.Stabilization : user.StrengtheningPreference.ToIntensityLevel();

        // Choose cooldown first
        var cooldownExercises = await GetCooldownExercises(user, todaysNewsletterRotation, token);
        var warmupExercises = await GetWarmupExercises(user, todaysNewsletterRotation, token, /*sa. Cat/Cow*/ excludeVariations: cooldownExercises.Select(vm => vm.Variation));

        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        // Refresh the core set once a month so the user can build strength without too much variety while not allowing stagnation to set in.
        var mainExercises = (await new ExerciseQueryBuilder(_context, refresh: needsFunctionalRefresh.needsRefresh)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
            })
            .WithMovementPatterns(todaysNewsletterRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
            })
            .WithExcludeExercises(x =>
            {
                // Exclude warmup so we don't get two of something such as Pushup Plus which is both a warmup and main exercise
                x.AddExcludeVariations(warmupExercises.Select(e => e.Variation));
            })
            .WithExerciseType(ExerciseType.Main)
            // No cardio, strengthening exercises only. No isometric, we're wanting to work functional movements.
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.MuscleTarget)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Main, token))
            .ToList();

        var extraExercises = new List<ExerciseViewModel>();
        // If the user expects accessory exercises (and no adjunct) and has a deload week, don't show them the accessory exercises.
        // User is new to fitness? Don't add additional accessory exercises to the core set.
        // If the user expects adjunct (even with a deload week), show them the accessory exercises.
        bool populateExtraMain = !user.IsNewToFitness && (user.IncludeAdjunct || !needsDeload.needsDeload);
        // If the user expects adjunct and has a deload week, don't show them the adjunct section. 
        bool populateAdjunct = user.IncludeAdjunct && !needsDeload.needsDeload;
        if (populateExtraMain || populateAdjunct)
        {
            if (populateAdjunct)
            {
                // Grabs 1 plyometric exercise to start off the adjunct section, in case their heart rate has fallen.
                extraExercises.AddRange((await new ExerciseQueryBuilder(_context)
                    .WithUser(user)
                    .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
                    {
                        x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                    })
                    .WithProficency(x => {
                        x.DoCapAtProficiency = needsDeload.needsDeload;
                    })
                    // Exclude warmup because this is looking for plyometric and we don't want to use something from warmupCardio
                    .WithExcludeExercises(x =>
                    {
                        // sa. exclude the same Mountain Climber variation we worked for a warmup
                        x.AddExcludeVariations(warmupExercises.Select(vm => vm.Variation));
                        // sa. exclude the same Kettlebell Swings variation we worked for a hip hinge exercise
                        x.AddExcludeVariations(mainExercises.Select(vm => vm.Variation));
                    })
                    .WithExerciseType(ExerciseType.Main)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    // Start off with some vigor 
                    .WithMuscleMovement(MuscleMovement.Plyometric)
                    .WithRecoveryMuscle(MuscleGroups.None)
                    .WithSportsFocus(SportsFocus.None)
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Extra, token)));
            }

            // Grabs a full workout of accessory exercises.
            // The ones that work the same muscle groups that the core set
            // ... are moved to the adjunct section in case the user has a little something extra.
            // Refresh the core set once a month so the user can build strength without too much variety while not allowing stagnation to set in.
            var otherFull = await new ExerciseQueryBuilder(_context, refresh: needsAccessoryRefresh.needsRefresh)
                .WithUser(user)
                // Unset muscles that have already been worked twice or more by the main exercises
                .WithAlreadyWorkedMuscles(mainExercises.WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
                {
                    x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                    x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)todaysNewsletterRotation.MuscleGroups) > 6 ? 3 : 2;
                })
                .WithProficency(x => {
                    x.DoCapAtProficiency = needsDeload.needsDeload;
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
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles, skip: 1)
                .Build()
                .Query();

            var otherMain = new List<ExerciseViewModel>();
            foreach (var exercise in otherFull)
            {
                var musclesWorkedSoFar = otherMain.WorkedMuscles(vm => vm.Variation.StrengthMuscles, addition: mainExercises.WorkedMuscles(vm => vm.Variation.StrengthMuscles));
                var hasUnworkedMuscleGroup = (exercise.Variation.StrengthMuscles & todaysNewsletterRotation.MuscleGroups).UnsetFlag32(musclesWorkedSoFar & todaysNewsletterRotation.MuscleGroups) > MuscleGroups.None;
                if (hasUnworkedMuscleGroup)// || i == 0 /* Let one pass so we work our least used exercises eventually */)
                {
                    otherMain.Add(new ExerciseViewModel(exercise, todaysMainIntensityLevel, ExerciseTheme.Main, token));
                }
                else if (populateAdjunct)
                {
                    extraExercises.Add(new ExerciseViewModel(exercise, IntensityLevel.Endurance, ExerciseTheme.Extra, token));
                }
            }

            mainExercises.AddRange(otherMain.OrderByDescending(vm => BitOperations.PopCount((ulong)vm.Variation.StrengthMuscles)));
        }

        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        mainExercises.AddRange((await new ExerciseQueryBuilder(_context)
            .WithUser(user)
            // Unset muscles that have already been worked by the main exercises
            .WithAlreadyWorkedMuscles(mainExercises.Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
            .WithMuscleGroups(MuscleGroups.Core, x =>
            {
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                // Not null so we choose unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
            })
            .WithProficency(x => {
                x.DoCapAtProficiency = needsDeload.needsDeload;
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
            //.WithOrderBy(ExerciseQueryBuilder.OrderByEnum.UniqueMuscles)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Main, token)));

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
                    x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
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

        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload: needsDeload.needsDeload, needsFunctionalRefresh: needsFunctionalRefresh.needsRefresh, needsAccessoryRefresh: needsAccessoryRefresh.needsRefresh, mainExercises.Concat(extraExercises));
        var viewModel = new NewsletterViewModel(user, newsletter, token)
        {
            TimeUntilDeload = needsDeload.timeUntilDeload,
            TimeUntilFunctionalRefresh = needsFunctionalRefresh.timeUntilRefresh,
            TimeUntilAccessoryRefresh = needsAccessoryRefresh.timeUntilRefresh,
            AllEquipment = equipmentViewModel,
            RecoveryExercises = recoveryExercises,
            WarmupExercises = warmupExercises,
            MainExercises = mainExercises,
            ExtraExercises = extraExercises,
            SportsExercises = sportsExercises,
            CooldownExercises = cooldownExercises
        };

        await UpdateLastSeenDate(user, viewModel.AllExercises, viewModel.ExtraExercises);

        ViewData[ViewData_Deload] = needsDeload.needsDeload;
        return View(nameof(Newsletter), viewModel);
    }

    public async Task<IActionResult> DebugNewsletter(User user, string token)
    {
        var todaysNewsletterRotation = await GetTodaysNewsletterRotation(user);

        user.EmailVerbosity = Verbosity.Debug;
        IList<ExerciseViewModel> debugExercises = await GetDebugExercises(user, token, count: 1);

        var equipmentViewModel = new EquipmentViewModel(_context.Equipment.Where(e => e.DisabledReason == null), user.UserEquipments.Select(eu => eu.Equipment));
        var newsletter = await CreateAndAddNewsletterToContext(user, todaysNewsletterRotation, needsDeload: false, needsFunctionalRefresh: false, needsAccessoryRefresh: false, debugExercises);
        var viewModel = new NewsletterViewModel(user, newsletter, token)
        {
            AllEquipment = equipmentViewModel,
            WarmupExercises = new List<ExerciseViewModel>(0),
            MainExercises = new List<ExerciseViewModel>(0),
            ExtraExercises = new List<ExerciseViewModel>(0),
            CooldownExercises = new List<ExerciseViewModel>(0),
            DebugExercises = debugExercises
        };

        await UpdateLastSeenDate(user, viewModel.AllExercises, viewModel.ExtraExercises);

        ViewData[ViewData_Deload] = false;
        return View(nameof(Newsletter), viewModel);
    }
}
