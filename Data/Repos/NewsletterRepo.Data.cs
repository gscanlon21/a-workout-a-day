﻿using Core.Code.Extensions;
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
    internal async Task<List<ExerciseVariationDto>> GetWarmupExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null)
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
            .WithMuscleGroups(MuscleTargetsBuilder.WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsWithCore)
                .WithMuscleTargets(UserMuscleMobility.MuscleTargets.ToDictionary(kv => kv.Key, kv => context.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Mobility;
            })
            .WithExerciseType(ExerciseType.MobilityTraining)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        var warmupPotentiationOrPerformance = (await new QueryBuilder(Section.WarmupPotentiation)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            // This should work the same muscles we target in the workout.
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsWithCore)
                .WithoutMuscleTargets(), x =>
            {
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithExerciseType(ExerciseType.CardiovascularTraining)
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
            .Build()
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
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
                .WithMuscleGroups(context, MuscleGroupExtensions.All())
                .WithoutMuscleTargets(), x =>
            {
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles | vm.Variation.SecondaryMuscles;
            })
            .WithExerciseType(ExerciseType.CardiovascularTraining)
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
            .Build()
            .Query(serviceScopeFactory, take: 2))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        // Light cardio (jogging) should some before dynamic stretches (inch worms). Medium-intensity cardio (star jacks, fast feet) should come after.
        // https://www.scienceforsport.com/warm-ups/ (the RAMP method)
        return [.. warmupRaise, .. warmupActivationAndMobilization, .. warmupPotentiationOrPerformance];
    }

    #endregion
    #region Cooldown Exercises

    /// <summary>
    /// Returns a list of cooldown exercises.
    /// </summary>
    internal async Task<List<ExerciseVariationDto>> GetCooldownExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null)
    {
        var stretches = (await new QueryBuilder(Section.CooldownStretching)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, MuscleGroupExtensions.All())
                .WithMuscleTargets(UserMuscleFlexibility.MuscleTargets.ToDictionary(kv => kv.Key, kv => context.User.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)), x =>
            {
                // These are static stretches so only look at stretched muscles
                x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Flexibility;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.MobilityTraining)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        var mindfulness = (await new QueryBuilder(Section.Mindfulness)
            .WithUser(context.User)
            .WithExerciseType(ExerciseType.Mindfulness)
            .Build()
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        return [.. stretches, .. mindfulness];
    }

    #endregion
    #region Rehab Exercises

    /// <summary>
    /// Returns a list of rehabilitation exercises.
    /// </summary>
    internal async Task<IList<ExerciseVariationDto>> GetRehabExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null)
    {
        if (context.User.RehabFocus.As<MuscleGroups>() == MuscleGroups.None)
        {
            return [];
        }

        var rehabCooldown = (await new QueryBuilder(Section.RehabCooldown)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MuscleGroups>()])
                .WithoutMuscleTargets(), options =>
            {
                options.MuscleTarget = vm => vm.Variation.StretchMuscles;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Static)
            .WithMuscleMovement(MuscleMovement.Isometric)
            .Build()
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        var rehabWarmup = (await new QueryBuilder(Section.RehabWarmup)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MuscleGroups>()])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                x.AddExcludeVariations(rehabCooldown?.Select(vm => vm.Variation));
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Mobility)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .Build()
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        var rehabMain = (await new QueryBuilder(Section.RehabMain)
            .WithUser(context.User)
            .WithJoints(context.User.RehabFocus.As<Joints>())
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MuscleGroups>()])
                .WithoutMuscleTargets(), x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
            })
            .WithExerciseType(ExerciseType.Rehabilitation)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithMuscleContractions(MuscleContractions.Dynamic)
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic | MuscleMovement.Isometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                x.AddExcludeVariations(rehabCooldown?.Select(vm => vm.Variation));
                x.AddExcludeVariations(rehabWarmup?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();

        return rehabWarmup.Concat(rehabMain).Concat(rehabCooldown).ToList();
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    internal async Task<IList<ExerciseVariationDto>> GetSportsExercises(WorkoutContext context,
         IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null, IDictionary<MuscleGroups, int>? _ = null)
    {
        // Hide this section while deloading, so we get pure accessory exercises instead.
        if (context.User.SportsFocus == SportsFocus.None || context.NeedsDeload)
        {
            return [];
        }

        var sportsPlyo = (await new QueryBuilder(Section.SportsPlyometric)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups()
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload))
            .WithExerciseType(ExerciseType.SportsTraining)
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
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload));

        var sportsStrength = (await new QueryBuilder(Section.SportsStrengthening)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups()
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload))
            .WithExerciseType(ExerciseType.SportsTraining)
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
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload));

        return sportsPlyo.Concat(sportsStrength).ToList();
    }

    #endregion
    #region Core Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<ExerciseVariationDto>> GetCoreExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null, IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return (await new QueryBuilder(Section.Core)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.CoreMuscleGroups)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                // Adjust down when the user is in a deload week or when the user has a regular strengthening workout. For mobility workouts we generally always want a core exercise.
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload, adjustDownBuffer: context.NeedsDeload, adjustDown: context.Frequency != Frequency.OffDayStretches), x =>
            {
                // No core isolation exercises.
                x.AtLeastXMusclesPerExercise = 2;
                x.AtLeastXUniqueMusclesPerExercise = 2;
            })
            // Using the exercise type to filter out functional movements that shouldn't show in the core section.
            .WithExerciseType(ExerciseType.CoreTraining)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .Build()
            .Query(serviceScopeFactory, take: context.User.IncludeMobilityWorkouts ? 1 : 2))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<ExerciseVariationDto>> GetPrehabExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null)
    {
        if (context.User.PrehabFocus == PrehabFocus.None)
        {
            return [];
        }

        bool strengthening = context.Frequency != Frequency.OffDayStretches;
        var results = new List<ExerciseVariationDto>();
        foreach (var eVal in EnumExtensions.GetValuesExcluding32(PrehabFocus.None, PrehabFocus.All).Where(v => context.User.PrehabFocus.HasFlag(v)))
        {
            results.AddRange((await new QueryBuilder(strengthening ? Section.PrehabStrengthening : Section.PrehabStretching)
                .WithUser(context.User)
                .WithJoints(eVal.As<Joints>(), options =>
                {
                    options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
                })
                .WithMuscleGroups(MuscleTargetsBuilder
                    .WithMuscleGroups(context, [eVal.As<MuscleGroups>()])
                    .WithoutMuscleTargets(), x =>
                {
                    // Try to work isolation exercises (for muscle groups, not joints)? x.AtMostXUniqueMusclesPerExercise = 1; Reverse the loop in the QueryRunner and increment.
                    x.MuscleTarget = strengthening ? vm => vm.Variation.StrengthMuscles
                                                   : vm => vm.Variation.StretchMuscles;
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
                .Query(serviceScopeFactory, take: 1))
                // Not using a strengthening intensity level because we don't want these tracked by the weekly muscle volume tracker.
                .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            );
        }

        return results;
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Grabs a core set of compound exercises that work the functional movement patterns for the day.
    /// </summary>
    internal async Task<IList<ExerciseVariationDto>> GetFunctionalExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null)
    {
        return (await new QueryBuilder(Section.Functional)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            // Overuse of muscle targets are not checked, we always want to work the functional movement patterns.
            // Accessory and Sports exercises are enough to keep us in range until the functional exercises refresh.
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, MuscleGroupExtensions.All())
                .WithoutMuscleTargets())
            .WithMovementPatterns(context.WorkoutRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // No isometric, we're wanting to work functional movements. No plyometric, those are too intense for strength training outside of sports focus.
            .WithMuscleMovement(MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .WithExerciseType(ExerciseType.FunctionalTraining)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r, context.User.Intensity, context.NeedsDeload))
            .ToList();
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    internal async Task<IList<ExerciseVariationDto>> GetAccessoryExercises(WorkoutContext context,
        IEnumerable<ExerciseVariationDto>? excludeGroups = null, IEnumerable<ExerciseVariationDto>? excludeExercises = null, IEnumerable<ExerciseVariationDto>? excludeVariations = null, IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        // If the user doesn't have enough data adjust workouts by weekly muscle targets,
        // ... then skip accessory exercises because the default muscle targets for new users are halved.
        // Including accessory exercises right off the bat will overwork those.
        if (context.WeeklyMusclesWeeks <= UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
        {
            return [];
        }

        return (await new QueryBuilder(Section.Accessory)
            .WithUser(context.User)
            .WithJoints(Joints.None, options =>
            {
                options.ExcludeJoints = context.User.RehabFocus.As<Joints>();
            })
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload), x =>
            {
                x.SecondaryMuscleTarget = vm => vm.Variation.SecondaryMuscles;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Accessory;
            })
            // Using the exercise type to filter out functional movements that shouldn't show in the accessory section.
            .WithExerciseType(ExerciseType.AccessoryTraining)
            .WithExerciseFocus(ExerciseFocus.Strength)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // No plyometric, leave those to sports-focus or warmup-cardio
            .WithMuscleMovement(MuscleMovement.Isometric | MuscleMovement.Isotonic | MuscleMovement.Isokinetic)
            .Build()
            .Query(serviceScopeFactory))
            .Select(e => new ExerciseVariationDto(e, context.User.Intensity, context.NeedsDeload))
            .ToList();
    }

    #endregion
    #region Debug Exercises

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<List<ExerciseVariationDto>> GetDebugExercises(User user)
    {
        return (await new QueryBuilder(Section.Debug)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .Build()
            .Query(serviceScopeFactory, take: 1))
            .Select(r => new ExerciseVariationDto(r))
            .ToList();
    }

    #endregion
}
