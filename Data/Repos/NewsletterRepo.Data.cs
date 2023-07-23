using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Data.Query;
using Data.Dtos.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Data.Repos;

public partial class NewsletterRepo
{
    #region Warmup Exercises

    /// <summary>
    /// Returns a list of warmup exercises.
    /// </summary>
    internal async Task<List<ExerciseDto>> GetWarmupExercises(WorkoutContext context,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupActivationAndMobilization = (await new QueryBuilder(Section.WarmupActivationMobilization)
            .WithUser(context.User)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                var muscleTargets = UserMuscleMobility.MuscleTargets.Where(kv => context.WorkoutRotation.MuscleGroupsWithCore.HasFlag(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => context.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value);

                x.MuscleTargets = muscleTargets;
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (muscleTargets.Count(mt => mt.Value > 0) / 5));
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, context.User.Verbosity, IntensityLevel.Warmup))
            .ToList();

        var warmupPotentiationOrPerformance = (await new QueryBuilder(Section.WarmupPotentiationPerformance)
            .WithUser(context.User)
            // This should work the same muscles we target in the workout.
            .WithMuscleGroups(context.WorkoutRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, context.User.Verbosity, IntensityLevel.Warmup))
            .ToList();

        // Get the heart rate up. Can work any muscle.
        var warmupRaise = (await new QueryBuilder(Section.WarmupRaise)
            .WithUser(context.User)
            // We just want to get the blood flowing. It doesn't matter what muscles these work.
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, context.User.Verbosity, IntensityLevel.Warmup))
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
    internal async Task<List<ExerciseDto>> GetCooldownExercises(WorkoutContext context,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        var stretches = (await new QueryBuilder(Section.CooldownStretching)
            .WithUser(context.User)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                var muscleTargets = UserMuscleMobility.MuscleTargets.Where(kv => context.WorkoutRotation.MuscleGroupsWithCore.HasFlag(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => context.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value);

                x.MuscleTargets = muscleTargets;
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                // These are static stretches so only look at stretched muscles
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (muscleTargets.Count(mt => mt.Value > 0) / 7));
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Cooldown, context.User.Verbosity, IntensityLevel.Cooldown))
            .ToList();

        var mindfulness = (await new QueryBuilder(Section.Mindfulness)
            .WithUser(context.User)
            .WithExerciseType(ExerciseType.Mindfulness, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.Mindfulness | ExerciseType.Stretching;
            })
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Cooldown, context.User.Verbosity, IntensityLevel.Cooldown))
            .ToList();

        return stretches.Concat(mindfulness).ToList();
    }

    #endregion
    #region Rehab Exercises

    /// <summary>
    /// Returns a list of rehabilitation exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetRehabExercises(WorkoutContext context)
    {
        if (context.User.RehabFocus.As<MuscleGroups>() == MuscleGroups.None)
        {
            return new List<ExerciseDto>();
        }

        var rehabCooldown = (await new QueryBuilder(Section.RehabCooldown)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(context.User.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, context.User.Verbosity, IntensityLevel.Recovery))
            .ToList();

        var rehabWarmup = (await new QueryBuilder(Section.RehabWarmup)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(context.User.RehabFocus.As<MuscleGroups>(), x =>
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
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithSportsFocus(SportsFocus.None)
            .WithOnlyWeights(false)
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, context.User.Verbosity, IntensityLevel.Warmup))
            .ToList();

        var rehabMain = (await new QueryBuilder(Section.RehabMain)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(context.User.RehabFocus.As<MuscleGroups>(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
            })
            .WithExcludeExercises(x => { })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic | MuscleMovement.Isometric)
            .WithSportsFocus(SportsFocus.None)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeVariations(rehabCooldown?.Select(vm => vm.Variation));
                x.AddExcludeVariations(rehabWarmup?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, context.User.Verbosity, IntensityLevel.Recovery))
            .ToList();

        return rehabWarmup.Concat(rehabMain).Concat(rehabCooldown).ToList();
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetSportsExercises(WorkoutContext context,
         IDictionary<MuscleGroups, int> workedMusclesDict, IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        // Hide this section while deloading so we get pure accessory exercises instead.
        if (context.User.SportsFocus == SportsFocus.None || context.NeedsDeload)
        {
            return new List<ExerciseDto>();
        }

        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Base 1 target for each muscle group. If we've already worked this muscle, reduce the muscle target volume.
            .ToDictionary(mg => mg, mg => 1 - (workedMusclesDict.TryGetValue(mg, out int workedAmt) ? workedAmt : 0));

        var sportsPlyo = (await new QueryBuilder(Section.SportsPlyometric)
            .WithUser(context.User)
            .WithMuscleGroups(context.WorkoutRotation.MuscleGroupsSansCore, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.MuscleTargets = AdjustMuscleTargets(context, muscleTargets, adjustUp: false);
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = context.NeedsDeload;
            })
            .WithExerciseType(ExerciseType.SportsTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.SportsTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Strength | ExerciseFocus.Power | ExerciseFocus.Endurance | ExerciseFocus.Stability | ExerciseFocus.Agility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(context.User.SportsFocus)
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Other, context.User.Verbosity, ToIntensityLevel(context.User.IntensityLevel, context.NeedsDeload)));

        var sportsStrength = (await new QueryBuilder(Section.SportsStrengthening)
            .WithUser(context.User)
            .WithMuscleGroups(context.WorkoutRotation.MuscleGroupsSansCore, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.MuscleTargets = AdjustMuscleTargets(context, muscleTargets, adjustUp: false);
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = context.NeedsDeload;
            })
            .WithExerciseType(ExerciseType.SportsTraining, options =>
            {
                options.PrerequisiteExerciseType = ExerciseType.ResistanceTraining | ExerciseType.Stretching | ExerciseType.SportsTraining;
            })
            .WithExerciseFocus(ExerciseFocus.Strength | ExerciseFocus.Power | ExerciseFocus.Endurance | ExerciseFocus.Stability | ExerciseFocus.Agility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithSportsFocus(context.User.SportsFocus)
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Other, context.User.Verbosity, ToIntensityLevel(context.User.IntensityLevel, context.NeedsDeload)));

        return sportsPlyo.Concat(sportsStrength).ToList();
    }

    #endregion
    #region Core Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetCoreExercises(WorkoutContext context,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Base 1 target for each core muscle group.
            // If we're not a core then don't work it, but keep it in the muscle target list so that it can be excluded if overworked.
            .ToDictionary(mg => mg, mg => MuscleGroups.Core.HasFlag(mg) ? 1 : 0);

        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(Section.Core)
            .WithUser(context.User)
            .WithMuscleGroups(MuscleGroups.Core, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.MuscleTargets = AdjustMuscleTargets(context, muscleTargets, adjustUp: false);
                // We don't want to work just one core muscle at a time because that is prime for muscle imbalances
                x.AtLeastXMusclesPerExercise = 2;
                x.AtLeastXUniqueMusclesPerExercise = 1;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = context.NeedsDeload;
            })
            .WithExerciseType(ExerciseType.CoreTraining, options =>
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
            .Select(r => new ExerciseDto(r, ExerciseTheme.Main, context.User.Verbosity, ToIntensityLevel(context.User.IntensityLevel, context.NeedsDeload)))
            .ToList();
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetPrehabExercises(WorkoutContext context, bool strengthening,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        if (context.User.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseDto>();
        }

        var results = new List<ExerciseDto>();
        foreach (var eVal in EnumExtensions.GetValuesExcluding32(PrehabFocus.None, PrehabFocus.All).Where(v => context.User.PrehabFocus.HasFlag(v)))
        {
            results.AddRange((await new QueryBuilder(strengthening ? Section.PrehabStrengthening : Section.PrehabCooldown)
                .WithUser(context.User)
                .WithJoints(eVal.As<Joints>())
                .WithMuscleGroups(eVal.As<MuscleGroups>(), x =>
                {
                    // Try to work isolation exercises (for muscle groups, not joints)? x.AtMostXUniqueMusclesPerExercise = 1; Reverse the loop in the QueryRunner and increment.
                    x.MuscleTarget = strengthening ? vm => vm.Variation.StrengthMuscles
                                                   : vm => vm.Variation.StretchMuscles;
                    x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                })
                .WithProficency(x =>
                {
                    x.DoCapAtProficiency = context.NeedsDeload;
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
                .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, context.User.Verbosity, strengthening ? IntensityLevel.Recovery : IntensityLevel.Cooldown))
            );
        }

        return results;
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Returns a list of functional exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetFunctionalExercises(WorkoutContext context,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        return (await new QueryBuilder(Section.Functional)
            .WithUser(context.User)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
            })
            .WithMovementPatterns(context.WorkoutRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = context.NeedsDeload;
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
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Main, context.User.Verbosity, ToIntensityLevel(context.User.IntensityLevel, context.NeedsDeload)))
            .ToList();
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetAccessoryExercises(WorkoutContext context,
        IEnumerable<ExerciseDto> excludeGroups, IEnumerable<ExerciseDto> excludeExercises, IEnumerable<ExerciseDto> excludeVariations, IDictionary<MuscleGroups, int> workedMusclesDict)
    {
        // If the user has a deload week, don't show them the accessory exercises.
        if (context.NeedsDeload)
        {
            return new List<ExerciseDto>();
        }

        var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Base 1 target for each muscle group. If we've already worked this muscle, reduce the muscle target volume.
            .ToDictionary(mg => mg, mg => 1 - (workedMusclesDict.TryGetValue(mg, out int workedAmt) ? workedAmt : 0));

        return (await new QueryBuilder(Section.Accessory)
            .WithUser(context.User)
            .WithMuscleGroups(context.WorkoutRotation.MuscleGroups, x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.MuscleTargets = AdjustMuscleTargets(context, muscleTargets);
                x.SecondaryMuscleTarget = vm => vm.Variation.SecondaryMuscles;
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (BitOperations.PopCount((ulong)context.WorkoutRotation.MuscleGroups) / 6));
            })
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = context.NeedsDeload;
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
            .Build()
            .Query(_context))
            // Accessory exercises shouldn't be worked as hard as function movements--always lower the intensity.
            .Select(e => new ExerciseDto(e, ExerciseTheme.Main, context.User.Verbosity, ToIntensityLevel(context.User.IntensityLevel, lowerIntensity: true)))
            .ToList();
    }

    #endregion
    #region Data Helpers

    /// <summary>
    /// Adjustments to the muscle groups to reduce muscle imbalances.
    /// </summary>
    private static IDictionary<MuscleGroups, int> AdjustMuscleTargets(WorkoutContext context, IDictionary<MuscleGroups, int> muscleTargets, bool adjustUp = true, bool adjustDown = true)
    {
        IDictionary<MuscleGroups, Range> userMuscleTargetDefaults = UserMuscleStrength.MuscleTargets(context.User);
        if (context.WeeklyMuscles != null)
        {
            foreach (var key in muscleTargets.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (context.WeeklyMuscles[key].HasValue && userMuscleTargetDefaults.ContainsKey(key))
                {
                    // Use the default muscle target when the user's workout split never targets this muscle group--because they can't adjust this muscle group's muscle target.
                    var targetRange = (context.UserAllWorkedMuscles.HasFlag(key)
                        ? context.User.UserMuscleStrengths.FirstOrDefault(um => um.MuscleGroup == key)?.Range
                        : null) ?? userMuscleTargetDefaults[key];

                    // Don't be so harsh about what constitutes an out-of-range value when there is not a lot of weekly data to work with.
                    var spread = targetRange.End.Value - targetRange.Start.Value;
                    var adjustBy = Convert.ToInt32(Math.Max(ExerciseConsts.TargetVolumePerExercise, spread) / context.WeeklyMusclesWeeks);

                    // We don't work this muscle group often enough
                    if (adjustUp && context.WeeklyMuscles[key] < targetRange.Start.Value)
                    {
                        // Cap the muscle targets so we never get more than 2 accessory exercises a day for a specific muscle group.
                        muscleTargets[key] = Math.Min(2, muscleTargets[key] + ((targetRange.Start.Value - context.WeeklyMuscles[key].GetValueOrDefault()) / adjustBy) + 1);
                    }
                    // We work this muscle group too often
                    else if (adjustDown && context.WeeklyMuscles[key] > targetRange.End.Value)
                    {
                        // -1 means we don't choose any exercises that work this muscle. 0 means we don't specifically target this muscle, but exercises working other muscles may still be picked.
                        muscleTargets[key] = Math.Max(-1, muscleTargets[key] - ((context.WeeklyMuscles[key].GetValueOrDefault() - targetRange.End.Value) / adjustBy) - 1);
                    }
                }
            }
        }

        return muscleTargets;
    }

    #endregion
    #region Debug Exercises

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseDto>> GetDebugExercises(User user, int count = 1)
    {
        var baseQuery = _context.ExerciseVariations
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
                UserExercise = a.Exercise.UserExercises.FirstOrDefault(uv => uv.UserId == user.Id),
                UserExerciseVariation = a.UserExerciseVariations.FirstOrDefault(uv => uv.UserId == user.Id),
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.UserId == user.Id)
            }).AsNoTracking();

        return (await baseQuery.ToListAsync())
            .GroupBy(i => new { i.Exercise.Id, LastSeen = i.UserExercise?.LastSeen ?? DateOnly.MinValue })
            .OrderBy(a => a.Key.LastSeen)
            .Take(count)
            .SelectMany(e => e)
            .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
            .Select(r => new ExerciseDto(Section.None, r.Exercise, r.Variation, r.ExerciseVariation,
                r.UserExercise, r.UserExerciseVariation, r.UserVariation,
                easierVariation: (name: null, reason: null), harderVariation: (name: null, reason: null),
                intensityLevel: null, theme: ExerciseTheme.Main, verbosity: user.Verbosity))
            .ToList();
    }

    #endregion
}
