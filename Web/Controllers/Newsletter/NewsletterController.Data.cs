using Microsoft.EntityFrameworkCore;
using System.Numerics;
using Web.Code.Extensions;
using Web.Data.Query;
using Web.Entities.Exercise;
using Web.Entities.Newsletter;
using Web.Models.Exercise;
using Web.Models.User;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Newsletter;

public partial class NewsletterController
{
    #region Warmup Exercises

    /// <summary>
    /// Returns a list of warmup exercises.
    /// </summary>
    internal async Task<List<ExerciseViewModel>> GetWarmupExercises(Entities.User.User user, NewsletterRotation todaysNewsletterRotation, string token,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups | MuscleGroups.Core, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            //.WithAlreadyWorkedMuscles(warmupMovement.WorkedMuscles(muscleTarget: vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles))
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithMuscleContractions(MuscleContractions.Dynamic)
            //.WithMovementPatterns(MovementPattern.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithSportsFocus(SportsFocus.None)
            // Not checking .OnlyWeights(false) because some warmup exercises require weights to perform, such as Plate/Kettlebell Halos and Hip Weight Shift.
            //.WithOnlyWeights(false)
            //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles, skip: 1)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
            .ToList();

        // Get the heart rate up. Can work any muscle.
        // Ideal is 2-5 minutes. We want to provide at least 2x60s exercises.
        var warmupCardio = (await new QueryBuilder(_context)
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
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
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
    #region Cooldown Exercises

    /// <summary>
    /// Returns a list of cooldown exercises.
    /// </summary>
    internal async Task<List<ExerciseViewModel>> GetCooldownExercises(Entities.User.User user, NewsletterRotation todaysNewsletterRotation, string token,
        IEnumerable<Entities.Exercise.Exercise>? excludeExercises = null, IEnumerable<Variation>? excludeVariations = null)
    {
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups | MuscleGroups.Core, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)todaysNewsletterRotation.MuscleGroups) > 10 ? 3 : 2;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeExercises);
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
    #region Recovery Exercises

    /// <summary>
    /// Returns a list of recovery exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>?> GetRecoveryExercises(Entities.User.User user, string token)
    {
        if (user.RecoveryMuscle == MuscleGroups.None)
        {
            return null;
        }

        // Should recovery exercises target muscles in isolation?
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithProficency(x =>
            {
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
            .Concat((await new QueryBuilder(_context)
                .WithUser(user)
                .WithExerciseType(ExerciseType.Main)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithSportsFocus(SportsFocus.None)
                .WithRecoveryMuscle(user.RecoveryMuscle)
                .Build()
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Main, token)))
            .Concat((await new QueryBuilder(_context)
                .WithUser(user)
                .WithProficency(x =>
                {
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

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>?> GetSportsExercises(Entities.User.User user, string token, NewsletterRotation todaysNewsletterRotation, IntensityLevel todaysMainIntensityLevel, bool needsDeload)
    {
        if (user.SportsFocus == SportsFocus.None)
        {
            return null;
        }

        // Should recovery exercises target muscles in isolation?
        return (await new QueryBuilder(_context)
                .WithUser(user)
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
                {
                    x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = needsDeload;
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

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Returns a list of functional exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetFunctionalExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel todaysMainIntensityLevel, NewsletterRotation todaysNewsletterRotation,
        IEnumerable<Entities.Exercise.Exercise>? excludeExercises = null, IEnumerable<Variation>? excludeVariations = null)
    {
        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
            })
            .WithMovementPatterns(todaysNewsletterRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeExercises);
                x.AddExcludeExercises(excludeExercises);
                // Exclude warmup so we don't get two of something such as Pushup Plus which is both a warmup and main exercise
                x.AddExcludeVariations(excludeVariations);
            })
            .WithExerciseType(ExerciseType.Main)
            // No cardio, strengthening exercises only. No isometric, we're wanting to work functional movements.
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithOrderBy(OrderBy.MuscleTarget)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, todaysMainIntensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Accessory/Extra Exercises

    /// <summary>
    /// Returns a list of accessory/extra exercises.
    /// </summary>
    private async Task<(IList<ExerciseViewModel> accessory, IList<ExerciseViewModel> extra)> GetAccessoryExtraExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel todaysMainIntensityLevel, NewsletterRotation todaysNewsletterRotation,
        IEnumerable<ExerciseViewModel> excludeGroups, IEnumerable<ExerciseViewModel> excludeExercises, IEnumerable<ExerciseViewModel> excludeVariations, IDictionary<MuscleGroups, int> workedMusclesDict)
    {
        var extraExercises = new List<ExerciseViewModel>();
        var accessoryExercises = new List<ExerciseViewModel>();
        // If the user expects accessory exercises (and no adjunct) and has a deload week, don't show them the accessory exercises.
        // User is new to fitness? Don't add additional accessory exercises to the core set.
        // If the user expects adjunct (even with a deload week), show them the accessory exercises.
        bool populateExtraMain = !user.IsNewToFitness && (user.IncludeAdjunct || !needsDeload);
        // If the user expects adjunct and has a deload week, don't show them the adjunct section. 
        bool populateAdjunct = user.IncludeAdjunct && !needsDeload;
        if (populateExtraMain || populateAdjunct)
        {
            if (populateAdjunct)
            {
                // Grabs 1 plyometric exercise to start off the adjunct section, in case their heart rate has fallen.
                extraExercises.AddRange((await new QueryBuilder(_context)
                    .WithUser(user)
                    .WithMuscleGroups(todaysNewsletterRotation.MuscleGroups, x =>
                    {
                        x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                    })
                    .WithProficency(x =>
                    {
                        x.DoCapAtProficiency = needsDeload;
                    })
                    // Exclude warmup because this is looking for plyometric and we don't want to use something from warmupCardio
                    .WithExcludeExercises(x =>
                    {
                        // sa. exclude the same Mountain Climber variation we worked for a warmup
                        // sa. exclude the same Kettlebell Swings variation we worked for a hip hinge exercise
                        x.AddExcludeVariations(excludeVariations.Select(vm => vm.Variation));
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
            var otherFull = await new QueryBuilder(_context)
                .WithUser(user)
                .AddAlreadyWorkedMuscles(workedMusclesDict.Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                .WithMuscleGroups(todaysNewsletterRotation.MuscleGroupsWithCore, x =>
                {
                    x.ExcludeRecoveryMuscle = user.RecoveryMuscle;
                    x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)todaysNewsletterRotation.MuscleGroupsWithCore) > 9 ? 3 : 2;
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = needsDeload;
                })
                .WithExerciseType(ExerciseType.Main)
                .IsUnilateral(null)
                .WithExcludeExercises(x =>
                {
                    // sa. exclude Side Star Planks (working hip adductors) from the accessory set. Save those for the core section.
                    //x.AddExcludeGroups(ExerciseGroup.Planks);

                    x.AddExcludeGroups(excludeGroups.Select(vm => vm.Exercise));
                    x.AddExcludeExercises(excludeExercises.Select(vm => vm.Exercise));
                    x.AddExcludeVariations(excludeVariations.Select(vm => vm.Variation));
                })
                //.WithAlreadyWorkedMuscles(mainExercises.WorkedMuscles()) We want all muscles included so we have something for the adjunct section
                // Leave movement patterns to the first part of the main section - so we don't work a pull on a push day.
                .WithMovementPatterns(MovementPattern.None)
                // No cardio, strengthening exercises only
                .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                .WithSportsFocus(SportsFocus.None)
                .WithRecoveryMuscle(MuscleGroups.None)
                .WithOrderBy(OrderBy.UniqueMuscles, skip: 1)
                .Build()
                .Query();

            var otherMain = new List<ExerciseViewModel>();
            foreach (var exercise in otherFull)
            {
                var musclesWorkedSoFar = otherMain.WorkedMuscles(vm => vm.Variation.StrengthMuscles, addition: accessoryExercises.WorkedMuscles(vm => vm.Variation.StrengthMuscles, addition: workedMusclesDict.Where(kv => kv.Value >= 1).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key)));
                var hasUnworkedMuscleGroup = (exercise.Variation.StrengthMuscles & todaysNewsletterRotation.MuscleGroupsWithCore).UnsetFlag32(musclesWorkedSoFar & todaysNewsletterRotation.MuscleGroupsWithCore) > MuscleGroups.None;
                if (hasUnworkedMuscleGroup)// || i == 0 /* Let one pass so we work our least used exercises eventually (this was switched over the LastSeen date update) */)
                {
                    otherMain.Add(new ExerciseViewModel(exercise, todaysMainIntensityLevel, ExerciseTheme.Main, token));
                }
                else if (populateAdjunct)
                {
                    extraExercises.Add(new ExerciseViewModel(exercise, IntensityLevel.Endurance, ExerciseTheme.Extra, token));
                }
            }

            accessoryExercises.AddRange(otherMain
                // Order core exercises last
                .OrderBy(vm => BitOperations.PopCount((ulong)(vm.Variation.StrengthMuscles & MuscleGroups.Core)))
                // Then order by how many muscle groups each variation works
                .ThenByDescending(vm => BitOperations.PopCount((ulong)vm.Variation.StrengthMuscles))
            );
        }

        return (accessory: accessoryExercises, extra: extraExercises);
    }

    #endregion
}
