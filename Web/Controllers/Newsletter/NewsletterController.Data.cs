using Microsoft.EntityFrameworkCore;
using Web.Code.Extensions;
using Web.Data.Query;
using Web.Entities.Exercise;
using Web.Entities.Newsletter;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;
using Web.Services;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Newsletter;

public partial class NewsletterController
{
    #region Warmup Exercises

    /// <summary>
    /// Returns a list of warmup exercises.
    /// </summary>
    internal async Task<List<ExerciseViewModel>> GetWarmupExercises(Entities.User.User user, NewsletterRotation newsletterRotation, string token,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            .WithExerciseType(ExerciseType.Stretching, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            // Not checking .OnlyWeights(false) because some warmup exercises require weights to perform, such as Plate/Kettlebell Halos and Hip Weight Shift.
            //.WithOnlyWeights(false)
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
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithExerciseType(ExerciseType.CardiovasularTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.CardiovasularTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Endurance)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
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
    internal async Task<List<ExerciseViewModel>> GetCooldownExercises(Entities.User.User user, NewsletterRotation newsletterRotation, string token,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        var stretches = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // These are static stretches so only look at stretched muscles
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = newsletterRotation.IsFullBody ? 3 : 2;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Stretching, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Cooldown, ExerciseTheme.Cooldown, token))
            .ToList();

        var mindfulness = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithExerciseType(ExerciseType.Mindfulness, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.Mindfulness | ExerciseType.Stretching;
            })
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Cooldown, ExerciseTheme.Cooldown, token))
            .ToList();

        return stretches.Concat(mindfulness).ToList();
    }

    #endregion
    #region Recovery Exercises

    /// <summary>
    /// Returns a list of recovery exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetRecoveryExercises(Entities.User.User user, string token)
    {
        if (user.RehabFocus.As<MuscleGroups>() == MuscleGroups.None)
        {
            return new List<ExerciseViewModel>();
        }

        var rehabMain = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithJoints(user.RehabFocus.As<Joints>())
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
            })
            .WithExcludeExercises(x => { })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Extra, token))
            .ToList();

        var rehabCooldown = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithJoints(user.RehabFocus.As<Joints>())
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeVariations(rehabMain?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Extra, token))
            .ToList();

        var rehabWarmup = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithJoints(user.RehabFocus.As<Joints>())
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeVariations(rehabCooldown?.Select(vm => vm.Variation));
                x.AddExcludeVariations(rehabMain?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Extra, token))
            .ToList();

        return rehabWarmup.Concat(rehabMain).Concat(rehabCooldown).ToList();
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetSportsExercises(Entities.User.User user, string token, NewsletterRotation newsletterRotation, IntensityLevel intensityLevel, bool needsDeload,
         IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.SportsFocus == SportsFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        var sportsPlyo = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroupsSansCore, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.SportsTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.SportsTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Strength | ExerciseFocus.Power | ExerciseFocus.Endurance | ExerciseFocus.Stability | ExerciseFocus.Agility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(user.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Other, token));

        var sportsStrength = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroupsSansCore, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.SportsTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.SportsTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Strength | ExerciseFocus.Power | ExerciseFocus.Endurance | ExerciseFocus.Stability | ExerciseFocus.Agility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(user.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic | MuscleMovement.Isometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(sportsPlyo.Select(vm => vm.Exercise));
                x.AddExcludeVariations(sportsPlyo.Select(vm => vm.Variation));
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Other, token));

        return sportsPlyo.Concat(sportsStrength).ToList();
    }

    #endregion
    #region Core Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetCoreExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.Core, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // We don't want to work just one core muscle at a time because that is prime for muscle imbalances
                x.AtLeastXMusclesPerExercise = 2;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.ResistanceTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetPrehabExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, bool strengthening,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        var results = new List<ExerciseViewModel>();
        foreach (var eVal in EnumExtensions.GetValuesExcluding32(PrehabFocus.None, PrehabFocus.All).Where(v => user.PrehabFocus.HasFlag(v)))
        {
            results.AddRange((await new QueryBuilder(_context)
                .WithUser(user)
                .WithJoints(eVal.As<Joints>())
                .WithMuscleGroups(eVal.As<MuscleGroups>(), x =>
                {
                    // Try to work isolation exercises (for muscle groups, not joints)? x.AtMostXUniqueMusclesPerExercise = 1; Reverse the loop in the QueryRunner and increment.
                    x.MuscleTarget = strengthening ? vm => vm.Variation.StrengthMuscles
                                                   : vm => vm.Variation.StretchMuscles;
                    x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = needsDeload;
                })
                .WithExerciseType(ExerciseType.InjuryPrevention | ExerciseType.BalanceTraining)
                // Train mobility in total.
                .WithExerciseFocus(strengthening
                    ? (ExerciseFocus.Stability | ExerciseFocus.Strength)
                    : ExerciseFocus.Flexibility)
                .WithExcludeExercises(x =>
                {
                    x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                    x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                    x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    x.AddExcludeVariations(results?.Select(vm => vm.Variation));
                })
                // No cardio, strengthening exercises only
                .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                .Build()
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Extra, token))
            );
        }

        return results;
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Returns a list of functional exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetFunctionalExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, NewsletterRotation newsletterRotation,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithMovementPatterns(newsletterRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.ResistanceTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Strength)
            // No isometric, we're wanting to work functional movements. No plyometric, those are too intense for strength training outside of sports focus.
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithOrderBy(OrderBy.MuscleTarget)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetAccessoryExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, NewsletterRotation newsletterRotation,
        IEnumerable<ExerciseViewModel> excludeGroups, IEnumerable<ExerciseViewModel> excludeExercises, IEnumerable<ExerciseViewModel> excludeVariations, IDictionary<MuscleGroups, int> workedMusclesDict)
    {
        // If the user expects accessory exercises and has a deload week, don't show them the accessory exercises.
        // User is new to fitness? Don't add additional accessory exercises to the core set.
        if (user.IsNewToFitness || needsDeload)
        {
            return new List<ExerciseViewModel>();
        }

        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Only target muscles of our current rotation's muscle groups.
            .Where(mg => newsletterRotation.MuscleGroups.HasFlag(mg))
            // Base 1 target for each muscle group. If we've already worked this muscle, reduce the muscle target volume.
            .ToDictionary(mg => mg, mg => 1 - (workedMusclesDict.TryGetValue(mg, out int workedAmt) ? workedAmt : 0));

        // Adjustments to the muscle groups to reduce muscle imbalances.
        var weeklyMuscles = await _userService.GetWeeklyMuscleVolume(user, weeks: Math.Max(Entities.User.User.DeloadAfterEveryXWeeksDefault, user.DeloadAfterEveryXWeeks));
        if (weeklyMuscles != null)
        {
            foreach (var key in muscleTargets.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (weeklyMuscles[key].HasValue)
                {
                    var targetRange = user.UserMuscles.Cast<UserMuscle?>().FirstOrDefault(um => um?.MuscleGroup == key)?.Range ?? UserService.MuscleTargets[key];

                    // We work this muscle group too often
                    if (weeklyMuscles[key] > targetRange.End.Value)
                    {
                        muscleTargets[key] = muscleTargets[key] - Math.Max(1, (weeklyMuscles[key].GetValueOrDefault() - targetRange.End.Value) / Proficiency.TargetVolumePerExercise);
                    }
                    // We don't work this muscle group often enough
                    else if (weeklyMuscles[key] < targetRange.Start.Value)
                    {
                        muscleTargets[key] = muscleTargets[key] + Math.Max(1, (targetRange.Start.Value - weeklyMuscles[key].GetValueOrDefault()) / Proficiency.TargetVolumePerExercise);
                    }
                }
            }
        }

        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.None, x =>
            {
                x.MuscleTargets = muscleTargets;
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.AtLeastXUniqueMusclesPerExercise = newsletterRotation.IsFullBody ? 3 : 2;
                x.SecondaryMuscleTarget = vm => vm.Variation.SecondaryMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.ResistanceTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching;
            })
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations.Select(vm => vm.Variation));
            })
            // Leave movement patterns to the first part of the main section - so we don't work a pull on a push day.
            .WithMovementPatterns(MovementPattern.None)
            // No plyometric, leave those to sports-focus or warmup-cardio
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithOrderBy(OrderBy.CoreLast)
            .Build()
            .Query())
            .Select(e => new ExerciseViewModel(e, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
}
