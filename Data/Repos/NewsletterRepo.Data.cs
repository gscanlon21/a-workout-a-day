using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query;
using Data.Query.Builders;

namespace Data.Repos;

public partial class NewsletterRepo
{
    #region Warmup Exercises

    /// <summary>
    /// Returns a list of warmup exercises.
    /// </summary>
    internal async Task<List<QueryResults>> GetWarmupExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        // Removing warmupMovement because what is an upper body horizontal push warmup?
        // Also, when to do lunge/square warmup movements instead of, say, groiners?
        // The user can do a dry-run set of the regular workout w/o weight as a movement warmup.
        // Some warmup exercises require weights to perform, such as Plate/Kettlebell Halos and Hip Weight Shift.
        var warmupActivationAndMobilization = await new QueryBuilder(Section.WarmupActivationMobilization)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder.WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsWithCore)
                .WithMuscleTargets(UserMuscleMobility.MuscleTargets.ToDictionary(kv => kv.Key, kv => context.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stretches;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Mobility;
            })
            .WithExerciseFocus([ExerciseFocus.Flexibility, ExerciseFocus.Stability, ExerciseFocus.Strength, ExerciseFocus.Activation], options =>
            {
                options.ExcludeExerciseFocus = [ExerciseFocus.Speed];
            })
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory);

        var warmupPotentiationOrPerformance = await new QueryBuilder(Section.WarmupPotentiation)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            // This should work the same muscles we target in the workout.
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsWithCore)
                .WithoutMuscleTargets(), x =>
            {
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.Stretches | vm.Variation.Strengthens | vm.Variation.Stabilizes;
            })
            // Speed will filter down to either Speed, Agility, or Power variations (sa. fast feet, karaoke, or burpees).
            .WithExerciseFocus([ExerciseFocus.Speed])
            // Karaoke is not plyometric.
            //.WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        // Get the heart rate up. Can work any muscle.
        var warmupRaise = await new QueryBuilder(Section.WarmupRaise)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            // We just want to get the blood flowing. It doesn't matter what muscles these work.
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, UserMuscleMobility.MuscleTargets.Select(mt => mt.Key).ToList())
                .WithoutMuscleTargets(), x =>
            {
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.Stretches | vm.Variation.Strengthens | vm.Variation.Stabilizes;
            })
            .WithExerciseFocus([ExerciseFocus.Endurance], options =>
            {
                // No mountain climbers or karaoke.
                options.ExcludeExerciseFocus = [ExerciseFocus.Strength, ExerciseFocus.Speed];
            })
            // Supine Leg Cycle is not plyometric
            //.WithMuscleMovement(MuscleMovement.Plyometric)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                // Choose different exercises than the other warmup cardio exercises.
                x.AddExcludeVariations(warmupActivationAndMobilization.Select(vm => vm.Variation));
                x.AddExcludeExercises(warmupPotentiationOrPerformance.Select(vm => vm.Exercise));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 2);

        // Light cardio (jogging) should some before dynamic stretches (inch worms). Medium-intensity cardio (star jacks, fast feet) should come after.
        // https://www.scienceforsport.com/warm-ups/ (the RAMP method)
        return [.. warmupRaise, .. warmupActivationAndMobilization, .. warmupPotentiationOrPerformance];
    }

    #endregion
    #region Cooldown Exercises

    /// <summary>
    /// Returns a list of cooldown exercises.
    /// </summary>
    internal async Task<List<QueryResults>> GetCooldownExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        // These should be static stretches.
        var stretches = await new QueryBuilder(Section.CooldownStretching)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, MuscleGroupExtensions.All())
                .WithMuscleTargets(UserMuscleFlexibility.MuscleTargets.ToDictionary(kv => kv.Key, kv => context.User.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)), x =>
            {
                // We only want exercises that stretch muscles.
                x.MuscleTarget = vm => vm.Variation.Stretches;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Flexibility;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithExerciseFocus([ExerciseFocus.Flexibility, ExerciseFocus.Stability])
            .WithMuscleMovement(MuscleMovement.Static)
            .Build()
            .Query(_serviceScopeFactory);

        var mindfulness = await new QueryBuilder(Section.Mindfulness)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [MusculoskeletalSystem.Mind])
                .WithoutMuscleTargets(), x =>
            {
                // Active mindful meditation or mindful relaxation—we want it all.
                x.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stretches;
            })
            .WithExcludeExercises(x =>
            {
                // Don't work the same variation that we worked as a stretch.
                x.AddExcludeVariations(stretches?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        return [.. stretches, .. mindfulness];
    }

    #endregion
    #region Rehab Exercises

    /// <summary>
    /// Returns a list of rehabilitation exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetRehabExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        if (context.User.RehabFocus.As<MusculoskeletalSystem>() == MusculoskeletalSystem.None)
        {
            return [];
        }

        // Range of motion, muscle activation.
        var rehabMechanics = await new QueryBuilder(Section.RehabMechanics)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithSkills(context.User.RehabFocus.GetSkillType()?.SkillType, context.User.RehabSkills)
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MusculoskeletalSystem>()])
                .WithoutMuscleTargets(), options =>
            {
                options.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stretches;
            })
            .WithExerciseFocus([ExerciseFocus.Stability, ExerciseFocus.Flexibility, ExerciseFocus.Activation], options =>
            {
                options.ExcludeExerciseFocus = [ExerciseFocus.Strength, ExerciseFocus.Speed, ExerciseFocus.Endurance];
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        // Learning to tolerate the complex and chaotic real world environment.
        var rehabVelocity = await new QueryBuilder(Section.RehabVelocity)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithSkills(context.User.RehabFocus.GetSkillType()?.SkillType, context.User.RehabSkills)
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MusculoskeletalSystem>()])
                .WithoutMuscleTargets())
            .WithExerciseFocus([ExerciseFocus.Speed, ExerciseFocus.Endurance], options =>
            {
                options.ExcludeExerciseFocus = [ExerciseFocus.Strength];
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeGroups(rehabMechanics?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(rehabMechanics?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(rehabMechanics?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        // Get back to normal muscle output w/o other muscles compensating.
        var rehabStrength = await new QueryBuilder(Section.RehabStrengthening)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithSkills(context.User.RehabFocus.GetSkillType()?.SkillType, context.User.RehabSkills)
            .WithExerciseFocus([ExerciseFocus.Strength])
            .WithMuscleMovement(MuscleMovement.Dynamic | MuscleMovement.Static)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MusculoskeletalSystem>()])
                .WithoutMuscleTargets())
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeGroups(rehabMechanics?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(rehabMechanics?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(rehabMechanics?.Select(vm => vm.Variation));

                x.AddExcludeGroups(rehabVelocity?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(rehabVelocity?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(rehabVelocity?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        // Stretches and resilience we leave to PrehabFocus. User can have both selected if they want.
        return [.. rehabMechanics, .. rehabVelocity, .. rehabStrength];
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetSportsExercises(WorkoutContext context,
         IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null, IDictionary<MusculoskeletalSystem, int>? _ = null)
    {
        // Hide this section while deloading, so we get pure accessory exercises instead.
        if (context.User.SportsFocus == SportsFocus.None || context.NeedsDeload)
        {
            return [];
        }

        var sportsPlyo = await new QueryBuilder(Section.SportsPlyometric)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithoutMuscleTargets())
            .WithExerciseFocus([ExerciseFocus.Power, ExerciseFocus.Agility, ExerciseFocus.Stamina])
            .WithSportsFocus(context.User.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        var sportsStrength = await new QueryBuilder(Section.SportsStrengthening)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithoutMuscleTargets())
            .WithExerciseFocus([ExerciseFocus.Strength, ExerciseFocus.Stability])
            .WithSportsFocus(context.User.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Dynamic | MuscleMovement.Static)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeExercises(sportsPlyo.Select(vm => vm.Exercise));
                x.AddExcludeVariations(sportsPlyo.Select(vm => vm.Variation));
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .Build()
            .Query(_serviceScopeFactory, take: 1);

        return [.. sportsPlyo, .. sportsStrength];
    }

    #endregion
    #region Core Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetCoreExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null, IDictionary<MusculoskeletalSystem, int>? workedMusclesDict = null)
    {
        // Always include the accessory core exercise in the main section, regardless of a deload week or if the user is new to fitness.
        return await new QueryBuilder(Section.Core)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.CoreMuscleGroups)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                // Adjust down when the user is in a deload week or when the user has a regular strengthening workout. For mobility workouts we generally always want a core exercise.
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload, adjustDownBuffer: context.NeedsDeload, adjustDown: context.Frequency != Frequency.Mobility), x =>
            {
                // No core isolation exercises.
                x.AtLeastXMusclesPerExercise = 2;
                x.AtLeastXUniqueMusclesPerExercise = 2;
            })
            .WithExerciseFocus([ExerciseFocus.Strength])
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // No cardio, strengthening exercises only
            .WithMuscleMovement(MuscleMovement.Static | MuscleMovement.Dynamic)
            .Build()
            .Query(_serviceScopeFactory, take: context.User.IncludeMobilityWorkouts ? 1 : 2);
    }

    #endregion
    #region Prehab Exercises

    /// <summary>
    /// Returns a list of core exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetPrehabExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        if (context.User.PrehabFocus == PrehabFocus.None)
        {
            return [];
        }

        var prehabResults = new List<QueryResults>();
        bool strengthening = context.User.IncludeMobilityWorkouts ? context.Frequency != Frequency.Mobility : context.WorkoutRotation.Id % 2 != 0;
        foreach (var prehabFocus in EnumExtensions.GetValuesExcluding(PrehabFocus.None, PrehabFocus.All).Where(v => context.User.PrehabFocus.HasFlag(v)))
        {
            var skills = context.User.UserPrehabSkills.FirstOrDefault(s => s.PrehabFocus == prehabFocus);
            prehabResults.AddRange(await new QueryBuilder(strengthening ? Section.PrehabStrengthening : Section.PrehabStretching)
                .WithUser(context.User, needsDeload: context.NeedsDeload)
                .WithSkills(prehabFocus.GetSkillType()?.SkillType, skills?.Skills)
                .WithSelectionOptions(options =>
                {
                    options.AllRefreshed = skills?.AllRefreshed ?? options.AllRefreshed;
                })
                .WithMuscleGroups(MuscleTargetsBuilder
                    .WithMuscleGroups(context, [prehabFocus.As<MusculoskeletalSystem>()])
                    .WithoutMuscleTargets(), x =>
                {
                    // TODO? Try to work isolation exercises (for muscle groups, not joints):
                    // ... x.AtMostXUniqueMusclesPerExercise = 1; Reverse the loop in the QueryRunner and increment.
                    x.MuscleTarget = strengthening ? vm => vm.Variation.Strengthens : vm => vm.Variation.Stretches;
                })
                // Train mobility in total. Include activation in case their muscle is turned-off.
                .WithExerciseFocus(strengthening ? [ExerciseFocus.Stability, ExerciseFocus.Strength, ExerciseFocus.Activation] : [ExerciseFocus.Flexibility], options =>
                {
                    options.ExcludeExerciseFocus = !strengthening ? [ExerciseFocus.Strength] : null;
                })
                .WithExcludeExercises(x =>
                {
                    x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                    x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                    x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
                    x.AddExcludeVariations(prehabResults?.Select(vm => vm.Variation));
                })
                // No cardio, strengthening exercises only
                .WithMuscleMovement(MuscleMovement.Static | MuscleMovement.Dynamic)
                .Build()
                .Query(_serviceScopeFactory, take: skills?.SkillCount ?? UserConsts.PrehabCountDefault)
            );
        }

        return prehabResults;
    }

    #endregion
    #region Functional Exercises

    /// <summary>
    /// Grabs a core set of compound exercises that work the functional movement patterns for the day.
    /// </summary>
    internal async Task<IList<QueryResults>> GetFunctionalExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        // Hide this section while deloading, so we get pure accessory exercises instead.
        // Since accessory exercises are hidden if WeeklyMusclesWeeks is too low, let's show functional movements here if both are otherwise true.
        if (context.NeedsDeload && context.WeeklyMusclesWeeks > UserConsts.MuscleTargetsTakeEffectAfterXWeeks)
        {
            return [];
        }

        return await new QueryBuilder(Section.Functional)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
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
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithExerciseFocus([ExerciseFocus.Strength])
            .Build()
            .Query(_serviceScopeFactory);
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetAccessoryExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null, IDictionary<MusculoskeletalSystem, int>? workedMusclesDict = null)
    {
        // If the user doesn't have enough data adjust workouts by weekly muscle targets,
        // ... then skip accessory exercises because the default muscle targets for new users are halved.
        // ... Including accessory exercises right off the bat will overwork those.
        // Since functional exercises don't show when there are no movement patterns, show accessory exercises when both are true.
        if (context.WeeklyMusclesWeeks <= UserConsts.MuscleTargetsTakeEffectAfterXWeeks && context.WorkoutRotation.MovementPatterns != MovementPattern.None)
        {
            return [];
        }

        var rotations = await _userRepo.GetWeeklyRotations(context.User, context.User.Frequency);
        return await new QueryBuilder(Section.Accessory)
            .WithUser(context.User, needsDeload: context.NeedsDeload)
            .WithMuscleGroups(MuscleTargetsBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload, rotations: rotations), x =>
            {
                x.SecondaryMuscleTarget = vm => vm.Variation.Stabilizes;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Accessory;
            })
            .WithExerciseFocus([ExerciseFocus.Strength])
            .WithExcludeExercises(x =>
            {
                x.AddExcludeGroups(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // No plyometric, leave those to sports-focus or warmup-cardio
            .WithMuscleMovement(MuscleMovement.Static | MuscleMovement.Dynamic)
            .Build()
            .Query(_serviceScopeFactory);
    }

    #endregion
    #region Debug Exercises

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<IList<QueryResults>> GetDebugExercises(User user)
    {
        return await new QueryBuilder(Section.Debug)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, uniqueExercises: false)
            .Build()
            .Query(_serviceScopeFactory, take: 1);
    }

    #endregion
}
