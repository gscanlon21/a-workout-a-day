using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Dtos.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query.Builders;
using Microsoft.EntityFrameworkCore;

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
        // Some warmup exercises require weights to perform, such as Plate/Kettlebell Halos and Hip Weight Shift.
        var warmupActivationAndMobilization = (await new QueryBuilder(Section.WarmupActivationMobilization)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder.WithMuscleGroups(context.WorkoutRotation.MuscleGroupsWithCore)
                .WithMuscleTargets(UserMuscleMobility.MuscleTargets.ToDictionary(kv => kv.Key, kv => context.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (x.GetWorkedMuscleSum() / 5));
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
            .Build()
            .Query(_context))
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, context.User.Verbosity, IntensityLevel.Warmup))
            .ToList();

        var warmupPotentiationOrPerformance = (await new QueryBuilder(Section.WarmupPotentiationPerformance)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            // This should work the same muscles we target in the workout.
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.WorkoutRotation.MuscleGroupsWithCore)
                .WithoutMuscleTargets(), x =>
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
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Warmup, context.User.Verbosity, IntensityLevel.Warmup))
            .ToList();

        // Get the heart rate up. Can work any muscle.
        var warmupRaise = (await new QueryBuilder(Section.WarmupRaise)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            // We just want to get the blood flowing. It doesn't matter what muscles these work.
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(MuscleGroups.All)
                .WithoutMuscleTargets(), x =>
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
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(MuscleGroups.All)
                .WithMuscleTargets(UserMuscleFlexibility.MuscleTargets.ToDictionary(kv => kv.Key, kv => context.User.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                // These are static stretches so only look at stretched muscles
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                // Should return ~5 (+-2, okay to be very fuzzy) exercises regardless of if the user is working full-body or only half of their body.
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (x.GetWorkedMuscleSum() / 7));
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
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.User.RehabFocus.As<MuscleGroups>())
                .WithoutMuscleTargets(), options =>
            {
                options.MuscleTarget = vm => vm.Variation.StretchMuscles;
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
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, context.User.Verbosity, IntensityLevel.Recovery))
            .ToList();

        var rehabWarmup = (await new QueryBuilder(Section.RehabWarmup)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.User.RehabFocus.As<MuscleGroups>())
                .WithoutMuscleTargets(), x =>
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
            .Build()
            .Query(_context))
            .Take(1)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Extra, context.User.Verbosity, IntensityLevel.Warmup))
            .ToList();

        var rehabMain = (await new QueryBuilder(Section.RehabMain)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.User.RehabFocus.As<MuscleGroups>())
                .WithoutMuscleTargets(), x =>
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
         IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null, IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        // Hide this section while deloading so we get pure accessory exercises instead.
        if (context.User.SportsFocus == SportsFocus.None || context.NeedsDeload)
        {
            return new List<ExerciseDto>();
        }

        var sportsPlyo = (await new QueryBuilder(Section.SportsPlyometric)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                .AdjustMuscleTargets(context, adjustUp: !context.NeedsDeload), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
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
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                .AdjustMuscleTargets(context, adjustUp: !context.NeedsDeload), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
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
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null, IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(Section.Core)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.WorkoutRotation.CoreMuscleGroups)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                .AdjustMuscleTargets(context, adjustUp: !context.NeedsDeload), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                // No core isolation exercises.
                x.AtLeastXMusclesPerExercise = 2;
                x.AtLeastXUniqueMusclesPerExercise = 2;
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
            .Take(context.User.IsNewToFitness ? 1 : 2)
            .Select(r => new ExerciseDto(r, ExerciseTheme.Main, context.User.Verbosity, ToIntensityLevel(context.User.IntensityLevel, context.NeedsDeload)))
            .ToList();
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<ExerciseDto>> GetPrehabExercises(WorkoutContext context,
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null)
    {
        if (context.User.PrehabFocus == PrehabFocus.None)
        {
            return new List<ExerciseDto>();
        }

        bool strengthening = context.Frequency != Frequency.OffDayStretches;
        var results = new List<ExerciseDto>();
        foreach (var eVal in EnumExtensions.GetValuesExcluding32(PrehabFocus.None, PrehabFocus.All).Where(v => context.User.PrehabFocus.HasFlag(v)))
        {
            results.AddRange((await new QueryBuilder(strengthening ? Section.PrehabStrengthening : Section.PrehabStretching)
                .WithUser(context.User)
                .WithJoints(eVal.As<Joints>(), options =>
                {
                    options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
                })
                .WithMuscleGroups(MuscleTargetsBuilder
                    .WithMuscleGroups(eVal.As<MuscleGroups>())
                    .WithoutMuscleTargets(), x =>
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
        // Not checking muscle targets, we always want to work the functional movement patterns.
        //var muscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
        //    // Base 1 target for each muscle group. If we've already worked this muscle, reduce the muscle target volume.
        //    .ToDictionary(mg => mg, mg => 1);

        // Grabs a core set of compound exercises that work the functional movement patterns for the day.
        return (await new QueryBuilder(Section.Functional)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(MuscleGroups.All)
                .WithoutMuscleTargets(), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                // Not checking muscle targets, we always want to work the functional movement patterns.
                //x.MuscleTargets = AdjustMuscleTargets(context, muscleTargets, adjustUp: false, adjustDown: false);
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
        IEnumerable<ExerciseDto>? excludeGroups = null, IEnumerable<ExerciseDto>? excludeExercises = null, IEnumerable<ExerciseDto>? excludeVariations = null, IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        // If the user has a deload week, don't show them the accessory exercises.
        // If the user is new to fitness and doesn't have enough data adjust workouts by weekly muscle targets
        // , then skip accessory exercises because the default muscle targets for new users are halved. Including accessory exercises right off the bat will overwork those.
        if (context.NeedsDeload || (context.User.IsNewToFitness && context.WeeklyMusclesWeeks <= UserConsts.MuscleTargetsTakeEffectAfterXWeeks))
        {
            return new List<ExerciseDto>();
        }

        return (await new QueryBuilder(Section.Accessory)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                .AdjustMuscleTargets(context, adjustUp: !context.NeedsDeload), x =>
            {
                x.ExcludeRecoveryMuscle = context.User.RehabFocus.As<MuscleGroups>();
                x.SecondaryMuscleTarget = vm => vm.Variation.SecondaryMuscles;
                x.AtLeastXUniqueMusclesPerExercise = Math.Min(3, 1 + (x.GetWorkedMuscleSum() / 6));
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
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
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
