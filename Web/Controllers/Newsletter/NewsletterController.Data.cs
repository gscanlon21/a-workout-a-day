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
    internal async Task<List<ExerciseViewModel>> GetWarmupExercises(Entities.User.User user, NewsletterRotation newsletterRotation, string token,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupExercises = (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroups | MuscleGroups.Core, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            .WithExerciseFocus(ExerciseFocus.Stretching)
            .WithExerciseType(ExerciseType.Mobility)
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
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Work unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.StabilityMuscles;
            })
            .WithExerciseFocus(ExerciseFocus.CardiovasularTraining)
            .WithExerciseType(ExerciseType.Endurance)
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
        IEnumerable<Entities.Exercise.Exercise>? excludeExercises = null, IEnumerable<Variation>? excludeVariations = null)
    {
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(newsletterRotation.MuscleGroups | MuscleGroups.Core, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)newsletterRotation.MuscleGroups) > 10 ? 3 : 2;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeExercises);
                x.AddExcludeExercises(excludeExercises);
                x.AddExcludeVariations(excludeVariations);
            })
            //.WithAlreadyWorkedMuscles(cooldownMovement.WorkedMuscles(muscleTarget: vm => vm.Variation.StretchMuscles))
            //.WithMovementPatterns(MovementPattern.None)
            .WithExerciseFocus(ExerciseFocus.Stretching)
            .WithExerciseType(ExerciseType.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .WithSportsFocus(SportsFocus.None)
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
    private async Task<IList<ExerciseViewModel>> GetRecoveryExercises(Entities.User.User user, string token)
    {
        if (user.RehabFocus.As<MuscleGroups>() == MuscleGroups.None)
        {
            return new List<ExerciseViewModel>();
        }

        // Should recovery exercises target muscles in isolation?
        return (await new QueryBuilder(_context)
            .WithUser(user)
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                // Not null so we choose unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .WithExerciseFocus(ExerciseFocus.Rehabilitation)
            .WithExerciseType(ExerciseType.Mobility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query())
            .Take(1)
            .Select(r => new ExerciseViewModel(r, IntensityLevel.Warmup, ExerciseTheme.Warmup, token))
            .Concat((await new QueryBuilder(_context)
                .WithUser(user)
                .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                    // Not null so we choose unique exercises
                    x.AtLeastXUniqueMusclesPerExercise = 0;
                })
                .WithExerciseFocus(ExerciseFocus.Rehabilitation)
                .WithExerciseType(ExerciseType.Strength)
                .WithMuscleContractions(MuscleContractions.Dynamic)
                .WithSportsFocus(SportsFocus.None)
                .Build()
                .Query())
                .Take(1)
                .Select(r => new ExerciseViewModel(r, IntensityLevel.Recovery, ExerciseTheme.Main, token)))
            .Concat((await new QueryBuilder(_context)
                .WithUser(user)
                .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                    // Not null so we choose unique exercises
                    x.AtLeastXUniqueMusclesPerExercise = 0;
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = true;
                })
                .WithExerciseFocus(ExerciseFocus.Rehabilitation)
                .WithExerciseType(ExerciseType.Mobility)
                .WithMuscleContractions(MuscleContractions.Static)
                .WithSportsFocus(SportsFocus.None)
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
    private async Task<IList<ExerciseViewModel>> GetSportsExercises(Entities.User.User user, string token, NewsletterRotation newsletterRotation, IntensityLevel intensityLevel, bool needsDeload,
         IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.SportsFocus == SportsFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        return (await new QueryBuilder(_context)
                .WithUser(user)
                .WithMuscleGroups(newsletterRotation.MuscleGroups, x =>
                {
                    x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = needsDeload;
                })
                .WithExerciseFocus(ExerciseFocus.SportsTraining)
                .WithExerciseType(ExerciseType.Strength | ExerciseType.Power | ExerciseType.Endurance | ExerciseType.Stability | ExerciseType.Agility)
                .IsUnilateral(null)
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
                .Take(2)
                .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Other, token))
                .Concat((await new QueryBuilder(_context)
                    .WithUser(user)
                    .WithMuscleGroups(newsletterRotation.MuscleGroups, x =>
                    {
                        x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                    })
                    .WithProficency(x =>
                    {
                        x.DoCapAtProficiency = needsDeload;
                    })
                    .WithExerciseFocus(ExerciseFocus.SportsTraining)
                    .WithExerciseType(ExerciseType.Strength | ExerciseType.Power | ExerciseType.Endurance | ExerciseType.Stability | ExerciseType.Agility)
                    .IsUnilateral(null)
                    .WithMuscleContractions(MuscleContractions.Dynamic)
                    .WithSportsFocus(user.SportsFocus)
                    .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic | MuscleMovement.Isometric)
                    .WithExcludeExercises(x =>
                    {
                        x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                        x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                        x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    })
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Other, token))
                )
                .ToList();
    }

    #endregion
    #region Prehab Mobility Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetPrehabMobilityExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        var results = new List<ExerciseViewModel>();
        foreach (var eVal in EnumExtensions.GetNotNoneValues32<PrehabFocus>().Where(v => v.HasAnyFlag32(user.PrehabFocus)))
        {
            if (eVal.As<Joints>() != Joints.None)
            {
                results.AddRange((await new QueryBuilder(_context)
                    .WithUser(user)
                    // Unset muscles that have already been worked by the main exercises
                    //.WithAlreadyWorkedMuscles(functionalExercises.Concat(accessoryExercises).Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                    .WithProficency(x =>
                    {
                        x.DoCapAtProficiency = needsDeload;
                    })
                    .WithExerciseFocus(ExerciseFocus.InjuryPrevention)
                    .WithExerciseType(ExerciseType.Mobility)
                    .IsUnilateral(null)
                    .WithExcludeExercises(x =>
                    {
                        // sa. exclude all Plank variations if we already worked any Plank variation earlier
                        x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                        x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                        x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    })
                    .WithJoints(eVal.As<Joints>())
                    // No cardio, strengthening exercises only
                    .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                    //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles)
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
                );
            }

            if (eVal.As<MuscleGroups>() != MuscleGroups.None)
            {
                results.AddRange((await new QueryBuilder(_context)
                    .WithUser(user)
                    // Unset muscles that have already been worked by the main exercises
                    //.WithAlreadyWorkedMuscles(functionalExercises.Concat(accessoryExercises).Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                    .WithMuscleGroups(eVal.As<MuscleGroups>(), x =>
                    {
                        x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                        x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                        // Not null so we choose unique exercises
                        x.AtLeastXUniqueMusclesPerExercise = 0;
                    })
                    .WithProficency(x =>
                    {
                        x.DoCapAtProficiency = needsDeload;
                    })
                    .WithExerciseFocus(ExerciseFocus.InjuryPrevention)
                    .WithExerciseType(ExerciseType.Mobility)
                    .IsUnilateral(null)
                    .WithExcludeExercises(x =>
                    {
                        // sa. exclude all Plank variations if we already worked any Plank variation earlier
                        x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                        x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                        x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    })
                    .WithSportsFocus(SportsFocus.None)
                    .WithMovementPatterns(MovementPattern.None)
                    // No cardio, strengthening exercises only
                    .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                    //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles)
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
                );
            }
        }

        return results;
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
            // Unset muscles that have already been worked by the main exercises
            //.WithAlreadyWorkedMuscles(functionalExercises.Concat(accessoryExercises).Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
            .WithMuscleGroups(MuscleGroups.Core, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Not null so we choose unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseType(ExerciseType.Strength)
            .WithExerciseFocus(ExerciseFocus.ResistanceTraining)
            .IsUnilateral(null)
            .WithExcludeExercises(x =>
            {
                // sa. exclude all Plank variations if we already worked any Plank variation earlier
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles)
            .Build()
            .Query())
            .Take(2)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Prehab Strength Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetPrehabStrengthExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        var results = new List<ExerciseViewModel>();
        foreach (var eVal in EnumExtensions.GetNotNoneValues32<PrehabFocus>().Where(v => v.HasAnyFlag32(user.PrehabFocus)))
        {
            if (eVal.As<MuscleGroups>() != MuscleGroups.None)
            {
                results.AddRange((await new QueryBuilder(_context)
                    .WithUser(user)
                    // Unset muscles that have already been worked by the main exercises
                    //.WithAlreadyWorkedMuscles(functionalExercises.Concat(accessoryExercises).Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                    .WithMuscleGroups(eVal.As<MuscleGroups>(), x =>
                    {
                        x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                        x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                        // Not null so we choose unique exercises
                        x.AtLeastXUniqueMusclesPerExercise = 0;
                    })
                    .WithProficency(x =>
                    {
                        x.DoCapAtProficiency = needsDeload;
                    })
                    .WithExerciseFocus(ExerciseFocus.InjuryPrevention)
                    .WithExerciseType(ExerciseType.Strength)
                    .IsUnilateral(null)
                    .WithExcludeExercises(x =>
                    {
                        // sa. exclude all Plank variations if we already worked any Plank variation earlier
                        x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                        x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                        x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    })
                    .WithSportsFocus(SportsFocus.None)
                    .WithMovementPatterns(MovementPattern.None)
                    // No cardio, strengthening exercises only
                    .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                    //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles)
                    .Build()
                    .Query())
                    .Take(2)
                    .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
                );
            }
        }

        return results;
    }

    #endregion
    #region Rehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetRehabExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel,
        IEnumerable<ExerciseViewModel>? excludeGroups = null, IEnumerable<ExerciseViewModel>? excludeExercises = null, IEnumerable<ExerciseViewModel>? excludeVariations = null)
    {
        if (user.RehabFocus == RehabFocus.None)
        {
            return new List<ExerciseViewModel>();
        }

        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(_context)
            .WithUser(user)
            // Unset muscles that have already been worked by the main exercises
            //.WithAlreadyWorkedMuscles(functionalExercises.Concat(accessoryExercises).Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
            .WithMuscleGroups(user.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                //x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Not null so we choose unique exercises
                x.AtLeastXUniqueMusclesPerExercise = 0;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = needsDeload;
            })
            .WithExerciseFocus(ExerciseFocus.Rehabilitation)
            .WithExerciseType(ExerciseType.All)
            .IsUnilateral(null)
            .WithExcludeExercises(x =>
            {
                // sa. exclude all Plank variations if we already worked any Plank variation earlier
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithMovementPatterns(MovementPattern.None)
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles)
            .Build()
            .Query())
            .Take(2)
            .Select(r => new ExerciseViewModel(r, intensityLevel, ExerciseTheme.Main, token))
            .ToList();
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Returns a list of functional exercises.
    /// </summary>
    private async Task<IList<ExerciseViewModel>> GetFunctionalExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, NewsletterRotation newsletterRotation,
        IEnumerable<Entities.Exercise.Exercise>? excludeExercises = null, IEnumerable<Variation>? excludeVariations = null)
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
                x.AddExcludeGroups(excludeExercises);
                x.AddExcludeExercises(excludeExercises);
                // Exclude warmup so we don't get two of something such as Pushup Plus which is both a warmup and main exercise
                x.AddExcludeVariations(excludeVariations);
            })
            .WithExerciseFocus(ExerciseFocus.ResistanceTraining)
            .WithExerciseType(ExerciseType.Strength)
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
    #region Accessory/Extra Exercises

    /// <summary>
    /// Returns a list of accessory/extra exercises.
    /// </summary>
    private async Task<(IList<ExerciseViewModel> accessory, IList<ExerciseViewModel> extra)> GetAccessoryExtraExercises(Entities.User.User user, string token, bool needsDeload, IntensityLevel intensityLevel, NewsletterRotation newsletterRotation,
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
            // Grabs a full workout of accessory exercises.
            // The ones that work the same muscle groups that the core set
            // ... are moved to the adjunct section in case the user has a little something extra.
            var otherFull = await new QueryBuilder(_context)
                .WithUser(user)
                .AddAlreadyWorkedMuscles(workedMusclesDict.Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                .WithMuscleGroups(newsletterRotation.MuscleGroupsWithCore, x =>
                {
                    x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                    // IMPROVE: `x.AtLeastXUniqueMusclesPerExercise` doesn't apply since `.WithOrderBy(OrderBy.UniqueMuscles, skip: 1)` is set
                    x.AtLeastXUniqueMusclesPerExercise = BitOperations.PopCount((ulong)newsletterRotation.MuscleGroupsWithCore) > 9 ? 3 : 2;
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = needsDeload;
                })
                .WithExerciseFocus(ExerciseFocus.ResistanceTraining)
                .WithExerciseType(ExerciseType.Strength)
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
                .WithOrderBy(OrderBy.UniqueMuscles, skip: 1)
                .Build()
                .Query();

            var otherMain = new List<ExerciseViewModel>();
            foreach (var exercise in otherFull)
            {
                var musclesWorkedSoFar = otherMain.WorkedMuscles(vm => vm.Variation.StrengthMuscles, addition: accessoryExercises.WorkedMuscles(vm => vm.Variation.StrengthMuscles, addition: workedMusclesDict.Where(kv => kv.Value >= 1).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key)));
                var hasUnworkedMuscleGroup = (exercise.Variation.StrengthMuscles & newsletterRotation.MuscleGroupsWithCore).UnsetFlag32(musclesWorkedSoFar & newsletterRotation.MuscleGroupsWithCore) > MuscleGroups.None;
                if (hasUnworkedMuscleGroup)// || i == 0 /* Let one pass so we work our least used exercises eventually (this was switched over the LastSeen date update) */)
                {
                    otherMain.Add(new ExerciseViewModel(exercise, intensityLevel, ExerciseTheme.Main, token));
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

            if (populateAdjunct)
            {
                // Grabs 1 core exercise to finish off the workout.
                extraExercises.AddRange((await new QueryBuilder(_context)
                    .WithUser(user)
                    // Unset muscles that have already been worked by the main exercises
                    //.WithAlreadyWorkedMuscles(functionalExercises.Concat(accessoryExercises).Concat(extraExercises).WorkedMusclesDict(e => e.Variation.StrengthMuscles).Where(kv => kv.Value >= 2).Aggregate(MuscleGroups.None, (acc, c) => acc | c.Key))
                    .WithMuscleGroups(MuscleGroups.Core, x =>
                    {
                        x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                        // Not null so we choose unique exercises
                        x.AtLeastXUniqueMusclesPerExercise = 0;
                    })
                    .WithProficency(x =>
                    {
                        x.DoCapAtProficiency = needsDeload;
                    })
                    .WithExerciseFocus(ExerciseFocus.ResistanceTraining)
                    .WithExerciseType(ExerciseType.Strength)
                    .IsUnilateral(null)
                    .WithExcludeExercises(x =>
                    {
                        // sa. exclude all Plank variations if we already worked any Plank variation earlier
                        x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                        x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                        x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                        x.AddExcludeExercises(otherFull?.Select(vm => vm.Exercise));
                        x.AddExcludeVariations(otherFull?.Select(vm => vm.Variation));
                    })
                    .WithSportsFocus(SportsFocus.None)
                    .WithMovementPatterns(MovementPattern.None)
                    // No cardio, strengthening exercises only
                    .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
                    //.WithOrderBy(ExerciseQueryBuilder.OrderBy.UniqueMuscles)
                    .Build()
                    .Query())
                    .Take(1)
                    .Select(r => new ExerciseViewModel(r, IntensityLevel.Endurance, ExerciseTheme.Extra, token)));
            }
        }

        return (accessory: accessoryExercises, extra: extraExercises);
    }

    #endregion
}
