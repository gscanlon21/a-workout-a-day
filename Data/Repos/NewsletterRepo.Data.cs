﻿using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Data.Query;
using Data.Dtos.Newsletter;
using Data.Entities.Newsletter;
using Data.Entities.User;
using System.Numerics;

namespace Data.Repos;

public partial class NewsletterRepo
{
    #region Warmup Exercises

    /// <summary>
    /// Returns a list of warmup exercises.
    /// </summary>
    public async Task<List<ExerciseDto>> GetWarmupExercises(User user, WorkoutRotation workoutRotation,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupActivationAndMobilization = (await new QueryBuilder(Section.WarmupActivationMobilization)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.None, x =>
            {
                x.MuscleTargets = EnumExtensions.GetSingleValuesExcluding32(MuscleGroups.PelvicFloor, MuscleGroups.TibialisAnterior).Where(mg => workoutRotation.MuscleGroupsWithCore.HasFlag(mg))
                    .ToDictionary(mg => mg, mg => user.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == mg)?.Count ?? (UserMuscleMobility.MuscleTargets.TryGetValue(mg, out int countTmp) ? countTmp : 0));
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (BitOperations.PopCount((ulong)workoutRotation.MuscleGroups) / 6));
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
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, user.Verbosity, IntensityLevel.Warmup))
            .ToList();

        var warmupPotentiationOrPerformance = (await new QueryBuilder(Section.WarmupPotentiationPerformance)
            .WithUser(user)
            // This should work the same muscles we target in the workout.
            .WithMuscleGroups(workoutRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithExerciseType(ExerciseType.CardiovasularTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.CardiovasularTraining;
            })
            // Speed will filter down to either Speed, Agility, or Power variations (sa. fast feet, karaoke, or burpees).
            .WithExerciseFocus(ExerciseFocus.Speed)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            // Karaoke is not plyometric.
            //.WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, user.Verbosity, IntensityLevel.Warmup))
            .ToList();

        // Get the heart rate up. Can work any muscle.
        var warmupRaise = (await new QueryBuilder(Section.WarmupRaise)
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
            .WithExerciseFocus(ExerciseFocus.Endurance, options =>
            {
                // No mountain climbers or karaoke.
                options.ExcludeExerciseFocus = ExerciseFocus.Strength | ExerciseFocus.Speed;
            })
            .WithMuscleContractions(MuscleContractions.Dynamic)
            // Supine Leg Cycle is not plyometric
            //.WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                // Choose different exercises than the other warmup cardio exercises.
                x.AddExcludeExercises(warmupPotentiationOrPerformance.Select(vm => vm.Exercise));
            })
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query(_context))
            .Take(2)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, user.Verbosity, IntensityLevel.Warmup))
            .ToList();

        // Light cardio (jogging) should some before dynamic stretches (inch worms). Medium-intensity cardio (star jacks, fast feet) should come after.
        // https://www.scienceforsport.com/warm-ups/ (the RAMP method)
        return warmupRaise.Concat(warmupActivationAndMobilization).Concat(warmupPotentiationOrPerformance).ToList();
    }

    #endregion
    #region Cooldown Exercises

    /// <summary>
    /// Returns a list of cooldown exercises.
    /// </summary>
    public async Task<List<ExerciseDto>> GetCooldownExercises(User user, WorkoutRotation workoutRotation,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        var stretches = (await new QueryBuilder(Section.CooldownStretching)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.None, x =>
            {
                x.MuscleTargets = EnumExtensions.GetSingleValuesExcluding32(MuscleGroups.PelvicFloor, MuscleGroups.TibialisAnterior).Where(mg => workoutRotation.MuscleGroupsWithCore.HasFlag(mg))
                    .ToDictionary(mg => mg, mg => user.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == mg)?.Count ?? (UserMuscleMobility.MuscleTargets.TryGetValue(mg, out int countTmp) ? countTmp : 0));
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                // These are static stretches so only look at stretched muscles
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (BitOperations.PopCount((ulong)workoutRotation.MuscleGroups) / 6));
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
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Cooldown, user.Verbosity, IntensityLevel.Cooldown))
            .ToList();

        var mindfulness = (await new QueryBuilder(Section.Mindfulness)
            .WithUser(user)
            .WithExerciseType(ExerciseType.Mindfulness, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.Mindfulness | ExerciseType.Stretching;
            })
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Cooldown, user.Verbosity, IntensityLevel.Cooldown))
            .ToList();

        return stretches.Concat(mindfulness).ToList();
    }

    #endregion
    #region Rehab Exercises

    /// <summary>
    /// Returns a list of rehabilitation exercises.
    /// </summary>
    public async Task<IList<ExerciseDto>> GetRehabExercises(User user)
    {
        if (user.RehabFocus.As<MuscleGroups>() == MuscleGroups.None)
        {
            return new List<ExerciseDto>();
        }

        var rehabMain = (await new QueryBuilder(Section.RehabMain)
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
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, user.Verbosity, IntensityLevel.Recovery))
            .ToList();

        var rehabCooldown = (await new QueryBuilder(Section.RehabCooldown)
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
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, user.Verbosity, IntensityLevel.Recovery))
            .ToList();

        var rehabWarmup = (await new QueryBuilder(Section.RehabWarmup)
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
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, user.Verbosity, IntensityLevel.Warmup))
            .ToList();

        return rehabWarmup.Concat(rehabMain).Concat(rehabCooldown).ToList();
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    public async Task<IList<ExerciseDto>> GetSportsExercises(User user, WorkoutRotation workoutRotation, IntensityLevel intensityLevel, bool needsDeload,
         IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        if (user.SportsFocus == SportsFocus.None)
        {
            return new List<ExerciseDto>();
        }

        var sportsPlyo = (await new QueryBuilder(Section.SportsPlyometric)
            .WithUser(user)
            .WithMuscleGroups(workoutRotation.MuscleGroupsSansCore, x =>
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
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Other, user.Verbosity, intensityLevel));

        var sportsStrength = (await new QueryBuilder(Section.SportsStrengthening)
            .WithUser(user)
            .WithMuscleGroups(workoutRotation.MuscleGroupsSansCore, x =>
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
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Other, user.Verbosity, intensityLevel));

        return sportsPlyo.Concat(sportsStrength).ToList();
    }

    #endregion
    #region Core Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    public async Task<IList<ExerciseDto>> GetCoreExercises(User user, MuscleGroups userAllWorkedMuscles, bool needsDeload, IntensityLevel intensityLevel, WorkoutRotation workoutRotation, IDictionary<MuscleGroups, int?>? weeklyMuscles,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Only target muscles of our current rotation's muscle groups.
            .Where(mg => workoutRotation.MuscleGroupsWithCore.HasFlag(mg))
            // Base 1 target for each core muscle group.
            // If we're not a core then don't work it, but keep it in the muscle target list so that it can be excluded if overworked.
            .ToDictionary(mg => mg, mg => MuscleGroups.Core.HasFlag(mg) ? 1 : 0);

        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(Section.Core)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.None, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.MuscleTargets = AdjustMuscleTargets(user, userAllWorkedMuscles, muscleTargets, weeklyMuscles, adjustUp: false);
                // We don't want to work just one core muscle at a time because that is prime for muscle imbalances
                x.AtLeastXMusclesPerExercise = 2;
                x.AtLeastXUniqueMusclesPerExercise = 1;
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
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Main, user.Verbosity, intensityLevel))
            .ToList();
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    public async Task<IList<ExerciseDto>> GetPrehabExercises(User user, bool needsDeload, bool strengthening,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        if (user.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseDto>();
        }

        var results = new List<ExerciseDto>();
        foreach (var eVal in EnumExtensions.GetValuesExcluding32(PrehabFocus.None, PrehabFocus.All).Where(v => user.PrehabFocus.HasFlag(v)))
        {
            results.AddRange((await new QueryBuilder(strengthening ? Section.PrehabStrengthening : Section.PrehabCooldown)
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
                .WithExerciseFocus(strengthening ? ExerciseFocus.Stability | ExerciseFocus.Strength : ExerciseFocus.Flexibility, options =>
                {
                    options.ExcludeExerciseFocus = !strengthening ? ExerciseFocus.Strength : null;
                })
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
                .Query(_context))
                .Take(1)
                // Not using a strengthening intensity level because we don't want these tracked by the weekly muscle volume tracker.
                .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, user.Verbosity, strengthening ? IntensityLevel.Recovery : IntensityLevel.Cooldown))
            );
        }

        return results;
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Returns a list of functional exercises.
    /// </summary>
    public async Task<IList<ExerciseDto>> GetFunctionalExercises(User user, bool needsDeload, IntensityLevel intensityLevel, WorkoutRotation workoutRotation,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        return (await new QueryBuilder(Section.Functional)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
            })
            .WithMovementPatterns(workoutRotation.MovementPatterns, x =>
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
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Main, user.Verbosity, intensityLevel))
            .ToList();
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    public async Task<IList<ExerciseDto>> GetAccessoryExercises(User user, MuscleGroups userAllWorkedMuscles, bool needsDeload, IntensityLevel intensityLevel, WorkoutRotation workoutRotation, IDictionary<MuscleGroups, int?>? weeklyMuscles,
        IEnumerable<ExerciseDto> excludeGroups, IEnumerable<ExerciseDto> excludeExercises, IEnumerable<ExerciseDto> excludeVariations, IDictionary<MuscleGroups, int> workedMusclesDict)
    {
        // If the user expects accessory exercises and has a deload week, don't show them the accessory exercises.
        // User is new to fitness? Don't add additional accessory exercises to the core set.
        if (user.IsNewToFitness || needsDeload)
        {
            return new List<ExerciseDto>();
        }

        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Only target muscles of our current rotation's muscle groups.
            .Where(mg => workoutRotation.MuscleGroups.HasFlag(mg))
            // Base 1 target for each muscle group. If we've already worked this muscle, reduce the muscle target volume.
            .ToDictionary(mg => mg, mg => 1 - (workedMusclesDict.TryGetValue(mg, out int workedAmt) ? workedAmt : 0));

        return (await new QueryBuilder(Section.Accessory)
            .WithUser(user)
            .WithMuscleGroups(MuscleGroups.None, x =>
            {
                x.ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
                x.MuscleTargets = AdjustMuscleTargets(user, userAllWorkedMuscles, muscleTargets, weeklyMuscles);
                x.SecondaryMuscleTarget = vm => vm.Variation.SecondaryMuscles;
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (BitOperations.PopCount((ulong)workoutRotation.MuscleGroups) / 6));
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
            .Query(_context))
            .Select(e => new ExerciseDto(e, ExerciseTheme.Main, user.Verbosity, intensityLevel))
            .ToList();
    }

    #endregion
    #region Data Helpers

    /// <summary>
    /// Adjustments to the muscle groups to reduce muscle imbalances.
    /// </summary>
    private static IDictionary<MuscleGroups, int> AdjustMuscleTargets(User user, MuscleGroups userAllWorkedMuscles, IDictionary<MuscleGroups, int> muscleTargets, IDictionary<MuscleGroups, int?>? weeklyMuscles, bool adjustUp = true, bool adjustDown = true)
    {
        if (weeklyMuscles != null)
        {
            foreach (var key in muscleTargets.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (weeklyMuscles[key].HasValue)
                {
                    // Use the default muscle target when the user's workout split never targets this muscle group--because they can't adjust this muscle group's muscle target.
                    var targetRange = (userAllWorkedMuscles.HasFlag(key)
                        ? user.UserMuscleStrengths.FirstOrDefault(um => um.MuscleGroup == key)?.Range
                        : null) ?? UserMuscleStrength.MuscleTargets[key];

                    // We don't work this muscle group often enough
                    if (adjustUp && weeklyMuscles[key] < targetRange.Start.Value)
                    {
                        muscleTargets[key] = muscleTargets[key] + ((targetRange.Start.Value - weeklyMuscles[key].GetValueOrDefault()) / ExerciseConsts.TargetVolumePerExercise) + 1;
                    }
                    // We work this muscle group too often
                    else if (adjustDown && weeklyMuscles[key] > targetRange.End.Value)
                    {
                        muscleTargets[key] = muscleTargets[key] - ((weeklyMuscles[key].GetValueOrDefault() - targetRange.End.Value) / ExerciseConsts.TargetVolumePerExercise) - 1;
                    }
                }
            }
        }

        return muscleTargets;
    }

    #endregion
}
