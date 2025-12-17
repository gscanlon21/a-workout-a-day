using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query;
using Data.Query.Builders;
using Data.Query.Builders.MuscleGroup;

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
        // Warmup movement patterns should work the joints involved through their full range of motion.
        // The user can also do a dry-run set of the regular workout w/o weight as a movement warmup.
        var warmupMobilization = context.User.ExtendedWarmup ? await new QueryBuilder(Section.WarmupMobilization)
           .WithUser(context.User, options =>
           {
               options.NeedsDeload = context.NeedsDeload;
               options.IgnorePrerequisites = context.User.IgnorePrerequisites;
           })
           .WithMovementPatterns(context.WorkoutRotation.MovementPatterns, x =>
           {
               x.IsUnique = true;
           })
           .WithExerciseFocus([ExerciseFocus.Flexibility, ExerciseFocus.Stability, ExerciseFocus.Strength, ExerciseFocus.Activation], options =>
           {
               options.ExcludeExerciseFocus = [ExerciseFocus.Speed];
           })
           .WithMuscleMovement(MuscleMovement.Dynamic)
           .WithExcludeExercises(x =>
           {
               x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
               x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
               x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
           })
           .WithSelectionOptions(options =>
           {
               options.UniqueExercises = true;
               options.Randomized = context.IsBackfill;
           })
           .Build()
           .Query(_serviceScopeFactory, OrderBy.None) : [];

        // Some warmup exercises require weights to perform, such as Halos.
        var warmupActivation = await new QueryBuilder(Section.WarmupActivation)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            }) // Only work muscles involved in the main workout.
            .WithMuscleGroups(MuscleGroupContextBuilder.WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsWithCore)
                .WithMuscleTargets(UserMuscleMobility.MuscleTargets.ToDictionary(kv => kv.Key, // Multiply count of exercises desired by expected volume worked by each.
                    kv => (context.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value) * ExerciseConsts.TargetVolumePerExercise)
                ), x =>
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
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeVariations(warmupMobilization.Select(vm => vm.Variation));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None);

        var warmupPotentiation = await new QueryBuilder(Section.WarmupPotentiation)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            // This should work the same muscles we target in the workout.
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsWithCore), x =>
            {
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.Stretches | vm.Variation.Strengthens | vm.Variation.Stabilizes;
            })
            // Speed will filter down to either Speed, Agility, or Power variations (sa. fast feet, karaoke, or burpees).
            .WithExerciseFocus([ExerciseFocus.Speed])
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        // Get the heart rate up. Can work any muscle.
        var warmupRaise = await new QueryBuilder(Section.WarmupRaise)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            // We just want to get the blood flowing. It doesn't matter what muscles these work.
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, UserMuscleMobility.MuscleTargets.Select(mt => mt.Key).ToList()), x =>
            {
                // Look through all muscle targets so that an exercise that doesn't work strength, if that is our only muscle target, still shows
                x.MuscleTarget = vm => vm.Variation.Stretches | vm.Variation.Strengthens | vm.Variation.Stabilizes;
            })
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithExerciseFocus([ExerciseFocus.Endurance], options =>
            {
                // No mountain climbers or karaoke. Include Supine Leg Cycle which is speed.
                options.ExcludeExerciseFocus = [ExerciseFocus.Strength, ExerciseFocus.Speed];
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                // Choose different exercises than the other warmup cardio exercises.
                x.AddExcludeVariations(warmupActivation.Select(vm => vm.Variation));
                x.AddExcludeVariations(warmupMobilization.Select(vm => vm.Variation));
                x.AddExcludeExercises(warmupPotentiation.Select(vm => vm.Exercise));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.LeastDifficultFirst, take: 2);

        // Light cardio (jogging) should some before dynamic stretches (inch worms). Medium-intensity cardio (star jacks, fast feet) should come after.
        // https://www.scienceforsport.com/warm-ups/ (the RAMP method)
        return [.. warmupRaise, .. warmupActivation, .. warmupMobilization, .. warmupPotentiation];
    }

    #endregion
    #region Cooldown Exercises

    /// <summary>
    /// Returns a list of cooldown exercises.
    /// </summary>
    internal async Task<List<QueryResults>> GetCooldownExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        // These should be yoga poses that aren't quite flexibility focused.
        var cooldownStabilization = await new QueryBuilder(Section.CooldownStabilization)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithMuscleGroups(MuscleGroupContextBuilder
                // Always cooldown all muscle groups.
                .WithMuscleGroups(context, MuscleGroupExtensions.All()), x =>
                {
                    // Make sure this variation works stabilizing muscles.
                    x.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stabilizes;
                })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // Include yoga arm balances, headstands, handstands, etc...
            .WithExerciseFocus([ExerciseFocus.Strength, ExerciseFocus.Stability], x =>
            {
                // Exclude stretching yoga poses (sa. Camel Stretch).
                x.ExcludeExerciseFocus = [ExerciseFocus.Flexibility];
            })
            .WithMuscleMovement(MuscleMovement.Static)
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        // These should be static stretches and yoga poses.
        var cooldownStretching = await new QueryBuilder(Section.CooldownStretching)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            }) // Always cooldown all muscle groups.
            .WithMuscleGroups(MuscleGroupContextBuilder.WithMuscleGroups(context, MuscleGroupExtensions.All())
                .WithMuscleTargets(UserMuscleFlexibility.MuscleTargets.ToDictionary(kv => kv.Key, // Multiply count of exercises desired by expected volume worked by each.
                    kv => (context.User.UserMuscleFlexibilities.SingleOrDefault(umf => umf.MuscleGroup == kv.Key)?.Count ?? kv.Value) * ExerciseConsts.TargetVolumePerExercise)
                ), x =>
            {
                // We only want exercises that stretch muscles.
                x.MuscleTarget = vm => vm.Variation.Stretches;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Flexibility;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeVariations(cooldownStabilization.Select(vm => vm.Variation));
            })
            .WithExerciseFocus([ExerciseFocus.Flexibility, ExerciseFocus.Stability])
            .WithMuscleMovement(MuscleMovement.Static)
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None);

        var cooldownRelaxation = await new QueryBuilder(Section.CooldownRelaxation)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithMuscleGroups(MuscleGroupContextBuilder // Include breathing exercises b/c they help the mind.
                .WithMuscleGroups(context, [MusculoskeletalSystem.Mind, MusculoskeletalSystem.Diaphragm]), x =>
            {
                // Include both active mindful meditations & passive mindful relaxations.
                x.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stretches;
            })
            .WithExcludeExercises(x =>
            {
                // Don't work the same exercise that we worked as a stretch (Dead Hangs).
                x.AddExcludeExercises(cooldownStabilization.Select(vm => vm.Exercise));
                x.AddExcludeExercises(cooldownStretching.Select(vm => vm.Exercise));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        return [.. cooldownStabilization, .. cooldownStretching, .. cooldownRelaxation];
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
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithSkills(context.User.RehabFocus.GetSkillType()?.Type, context.User.RehabSkills)
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MusculoskeletalSystem>()]), options =>
            {
                options.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stretches;
            })
            .WithExerciseFocus([ExerciseFocus.Stability, ExerciseFocus.Flexibility, ExerciseFocus.Activation], options =>
            {
                options.ExcludeExerciseFocus = [ExerciseFocus.Strength, ExerciseFocus.Speed, ExerciseFocus.Endurance];
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        // Learning to tolerate the complex and chaotic real world environment.
        var rehabVelocity = await new QueryBuilder(Section.RehabVelocity)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithSkills(context.User.RehabFocus.GetSkillType()?.Type, context.User.RehabSkills)
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MusculoskeletalSystem>()]))
            .WithExerciseFocus([ExerciseFocus.Speed, ExerciseFocus.Endurance], options =>
            {
                options.ExcludeExerciseFocus = [ExerciseFocus.Strength];
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeSkills(rehabMechanics.Select(vm => vm.Exercise));
                x.AddExcludeExercises(rehabMechanics.Select(vm => vm.Exercise));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        // Get back to normal muscle output w/o other muscles compensating.
        var rehabStrength = await new QueryBuilder(Section.RehabStrengthening)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithSkills(context.User.RehabFocus.GetSkillType()?.Type, context.User.RehabSkills)
            .WithExerciseFocus([ExerciseFocus.Strength])
            .WithMuscleMovement(MuscleMovement.Dynamic | MuscleMovement.Static)
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, [context.User.RehabFocus.As<MusculoskeletalSystem>()]))
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeSkills(rehabMechanics.Select(vm => vm.Exercise));
                x.AddExcludeExercises(rehabMechanics.Select(vm => vm.Exercise));

                x.AddExcludeSkills(rehabVelocity.Select(vm => vm.Exercise));
                x.AddExcludeExercises(rehabVelocity.Select(vm => vm.Exercise));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        // Stretches and resilience we leave to PrehabFocus. User can have both selected if they want.
        return [.. rehabMechanics, .. rehabVelocity, .. rehabStrength];
    }

    #endregion
    #region Sports Exercises

    /// <summary>
    /// Returns a list of sports exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetSportsExercises(WorkoutContext context,
         IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null)
    {
        // Hide this section while deloading, so we get pure accessory exercises instead.
        if (context.User.SportsFocus == SportsFocus.None || context.NeedsDeload)
        {
            return [];
        }

        var sportsPlyo = await new QueryBuilder(Section.SportsPower)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore))
            .WithExerciseFocus([ExerciseFocus.Power, ExerciseFocus.Agility, ExerciseFocus.Stamina])
            .WithSportsFocus(context.User.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Dynamic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

        var sportsStrength = await new QueryBuilder(Section.SportsStrengthening)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore))
            .WithExerciseFocus([ExerciseFocus.Strength, ExerciseFocus.Stability])
            .WithSportsFocus(context.User.SportsFocus)
            .WithMuscleMovement(MuscleMovement.Dynamic | MuscleMovement.Static)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                x.AddExcludeSkills(sportsPlyo.Select(vm => vm.Exercise));
                x.AddExcludeExercises(sportsPlyo.Select(vm => vm.Exercise));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None, take: 1);

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
        // Always include the core exercise, regardless of a deload week or if the user is new to fitness.
        return await new QueryBuilder(Section.Core)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.CoreMuscleGroups)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict)
                // AdjustDown when the user has a deload week or a regular strengthening workout. For mobility workouts, we generally always want a core exercise.
                // AdjustUpBuffer when deloading. So during normal workouts we can go above the RDA, then we remove the core exercise when above range or deloading.
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload, adjustUpBuffer: false /* .NeedsDeload */, adjustDown: context.Frequency != Frequency.Mobility), x =>
            {
                // Prefer to see a single core exercise.
                // Allows unseen core iso exercises too.
                x.AtLeastXUniqueMusclesPerExercise = 3;
            })
            // No cardio, strengthening exercises only.
            .WithExerciseFocus([ExerciseFocus.Strength, ExerciseFocus.Stability])
            .WithMuscleMovement(MuscleMovement.Static | MuscleMovement.Dynamic)
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build() // Max of two core exercises. Take one when deloading.
            .Query(_serviceScopeFactory, OrderBy.MusclesTargeted, take: context.NeedsDeload ? 1 : 2);
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
        // Let's try combining both strengthening and non-strengthening and let the user use the refresh padding to control how often each is seen.
        bool? strengthening = null; //context.User.IncludeMobilityWorkouts ? context.Frequency != Frequency.Mobility : context.WorkoutRotation.Id % 2 != 0; Randomize for max # break.
        foreach (var prehabFocus in EnumExtensions.GetValuesExcluding(PrehabFocus.None, PrehabFocus.All).Where(v => context.User.PrehabFocus.HasFlag(v)).OrderBy(_ => Guid.NewGuid()))
        {
            // Note that this doesn't return UseCaution exercises when the user is in a deload week.
            var skills = context.User.UserPrehabSkills.FirstOrDefault(s => s.PrehabFocus == prehabFocus);
            prehabResults.AddRange(await new QueryBuilder(strengthening.HasValue ? (strengthening.Value ? Section.PrehabStrengthening : Section.PrehabStretching) : Section.Prehab)
                .WithUser(context.User, options =>
                {
                    options.NeedsDeload = context.NeedsDeload;
                    options.IgnorePrerequisites = context.User.IgnorePrerequisites;
                })
                .WithSkills(prehabFocus.GetSkillType()?.Type, skills?.Skills)
                .WithMuscleGroups(MuscleGroupContextBuilder
                    .WithMuscleGroups(context, [prehabFocus.As<MusculoskeletalSystem>()]), x =>
                {
                    // TODO? Try to work isolation exercises (for muscle groups, not joints):
                    // ...x.AtMostXUniqueMusclesPerExercise = 1; Reverse the loop in the QueryRunner and increment.
                    x.MuscleTarget = strengthening.HasValue ? (strengthening.Value ? vm => vm.Variation.Strengthens : vm => vm.Variation.Stretches) : vm => vm.Variation.Strengthens | vm.Variation.Stretches;
                })
                // Train mobility in total. Include activation in case their muscle is too weak to function normally. Include speed and endurance for eye accommodative exercises and other odd stuff.
                .WithExerciseFocus(strengthening.HasValue // Cardio is filtered out by section.
                    ? (strengthening.Value ? [ExerciseFocus.Strength, ExerciseFocus.Stability, ExerciseFocus.Speed, ExerciseFocus.Endurance, ExerciseFocus.Activation] : [ExerciseFocus.Flexibility])
                    : [ExerciseFocus.Strength, ExerciseFocus.Stability, ExerciseFocus.Speed, ExerciseFocus.Endurance, ExerciseFocus.Activation, ExerciseFocus.Flexibility], options =>
                {
                    options.ExcludeExerciseFocus = strengthening == false ? [ExerciseFocus.Strength] : null;
                })
                .WithMuscleMovement(MuscleMovement.Static | MuscleMovement.Dynamic)
                .WithExcludeExercises(x =>
                {
                    x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                    x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                    x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));

                    x.AddExcludeSkills(prehabResults.Select(vm => vm.Exercise));
                    x.AddExcludeExercises(prehabResults.Select(vm => vm.Exercise));
                })
                .WithSelectionOptions(options =>
                {
                    options.UniqueExercises = true;
                    options.Randomized = context.IsBackfill;
                    options.AllRefreshed = skills?.AllRefreshed ?? options.AllRefreshed;
                })
                .Build()
                .Query(_serviceScopeFactory, OrderBy.None, take: skills?.SkillCount ?? UserConsts.PrehabCountDefault));

            // User's prefs means there may be a lot. Cap at half the max allowed # if the user is receiving two workouts per day.
            var maxPrehabExercises = ExerciseConsts.MaxPrehabExercisesPerWorkout / (context.User.SecondSendHour.HasValue ? 2 : 1);
            if (prehabResults.Count >= maxPrehabExercises) break;
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
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            // Overuse of muscle targets are not checked, we always want to work the functional movement patterns.
            // Accessory and Sports exercises are enough to keep us in range until the functional exercises refresh.
            .WithMuscleGroups(MuscleGroupContextBuilder.WithMuscleGroups(context, MuscleGroupExtensions.All()), options =>
            {
                // Carry exercises are all stabilization so they don't throw muscle targets off.
                options.MuscleTarget = vm => vm.Variation.Strengthens | vm.Variation.Stabilizes;
            })
            .WithMovementPatterns(context.WorkoutRotation.MovementPatterns, x =>
            {
                x.IsUnique = true;
            })
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            // No plyometric, those are too intense for strength training outside of sports focus.
            // No isometric, we're wanting to work functional movements.
            .WithMuscleMovement(MuscleMovement.Dynamic) // Carries are endurance.
            .WithExerciseFocus([ExerciseFocus.Strength, ExerciseFocus.Endurance])
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.PlyometricsFirst);
    }

    #endregion
    #region Accessory Exercises

    /// <summary>
    /// Returns a list of accessory exercises.
    /// </summary>
    internal async Task<IList<QueryResults>> GetAccessoryExercises(WorkoutContext context,
        IEnumerable<QueryResults>? excludeGroups = null, IEnumerable<QueryResults>? excludeExercises = null, IEnumerable<QueryResults>? excludeVariations = null, IDictionary<MusculoskeletalSystem, int>? workedMusclesDict = null)
    {
        // If the user doesn't have enough data to adjust workouts using the weekly muscle targets data,
        // ... then skip accessory exercises because the default muscle targets for new users are halved.
        // ... Including accessory exercises right off the bat will overwork those. The workout backfill will help.
        // Since functional exercises don't show when there are no movement patterns, show accessory exercises when both are true.
        if (context.WeeklyMusclesWeeks <= UserConsts.MuscleTargetsTakeEffectAfterXWeeks && context.WorkoutRotation.MovementPatterns != MovementPattern.None)
        {
            return [];
        }

        var rotations = await _userRepo.GetWeeklyRotations(context.User, context.User.Frequency);
        return await new QueryBuilder(Section.Accessory)
            .WithUser(context.User, options =>
            {
                options.NeedsDeload = context.NeedsDeload;
                options.IgnorePrerequisites = context.User.IgnorePrerequisites;
            })
            .WithMuscleGroups(MuscleGroupContextBuilder
                .WithMuscleGroups(context, context.WorkoutRotation.MuscleGroupsSansCore)
                .WithMuscleTargetsFromMuscleGroups(workedMusclesDict) // AdjustDownBuffer when deloading so we bring muscle targets back inline.
                .AdjustMuscleTargets(adjustUp: !context.NeedsDeload, adjustDownBuffer: context.NeedsDeload, rotations: rotations), x =>
            {
                x.SecondaryMuscleTarget = vm => vm.Variation.Stabilizes;
                x.AtLeastXUniqueMusclesPerExercise = context.User.AtLeastXUniqueMusclesPerExercise_Accessory;
            })
            // No plyometric, leave those to sports-focus or warmup-cardio
            .WithMuscleMovement(MuscleMovement.Static | MuscleMovement.Dynamic)
            .WithExerciseFocus([ExerciseFocus.Strength])
            .WithExcludeExercises(x =>
            {
                x.AddExcludeSkills(excludeGroups?.Select(vm => vm.Exercise));
                x.AddExcludeExercises(excludeExercises?.Select(vm => vm.Exercise));
                x.AddExcludeVariations(excludeVariations?.Select(vm => vm.Variation));
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = true;
                options.Randomized = context.IsBackfill;
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.CoreLast);
    }

    #endregion
    #region Debug Exercises

    /// <summary>
    /// Grab x-many exercises that the user hasn't seen in a long time.
    /// </summary>
    private async Task<IList<QueryResults>> GetDebugExercises(User user)
    {
        var exerciseVariations = await new QueryBuilder(Section.Debug)
            .WithUser(user, options =>
            {
                options.IgnoreProgressions = true;
                options.IgnorePrerequisites = true;
            })
            .WithSelectionOptions(options =>
            {
                options.UniqueExercises = false;
            })
            .Build().Query(_serviceScopeFactory);

        foreach (var exerciseVariation in exerciseVariations)
        {
            // An exercise with no instructions and no default instruction cannot be seen.
            if (string.IsNullOrWhiteSpace(exerciseVariation.Variation.DefaultInstruction)
                && exerciseVariation.Variation.Instructions.Count == 0)
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 1.");
            }

            // An exercise with a strength focus and no strengthened muscles may not be seen.
            if (exerciseVariation.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Strength)
                && exerciseVariation.Variation.Strengthens == MusculoskeletalSystem.None)
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 2.");
            }

            // An exercise with a stability focus and no stabilizing muscles may not be seen.
            if (exerciseVariation.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Stability)
                && exerciseVariation.Variation.Stabilizes == MusculoskeletalSystem.None)
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 3.");
            }

            // An exercise with a flexibility focus and no stretched muscles may not be seen.
            if (exerciseVariation.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Flexibility)
                && exerciseVariation.Variation.Stretches == MusculoskeletalSystem.None)
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 4.");
            }

            // A warmup exercise with no dynamic muscle movement may not be seen.
            if (!exerciseVariation.Variation.MuscleMovement.HasFlag(MuscleMovement.Dynamic)
                && exerciseVariation.Variation.Section.HasAnyFlag(Section.Warmup))
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 5.");
            }

            // A cooldown exercise with no static muscle movement may not be seen.
            if (!exerciseVariation.Variation.MuscleMovement.HasFlag(MuscleMovement.Static)
                && exerciseVariation.Variation.Section.HasAnyFlag(Section.Cooldown))
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 6.");
            }

            // An exercise both strengthens and stabilizes the same muscle — doubling up on muscle targets.
            if (exerciseVariation.Variation.Strengthens.HasAnyFlag(exerciseVariation.Variation.Stabilizes)
                // And the exercise works a section that uses muscle targets — they're actually doubled.
                && exerciseVariation.Variation.Section.HasAnyFlag(UserConsts.MuscleTargetSections))
            {
                UserLogs.Log(user, $"{exerciseVariation.Variation.Name} has an invalid configuration: 7.");
            }
        }

        return exerciseVariations
            .GroupBy(vm => vm.Exercise)
            .Take(1).SelectMany(g => g)
            .OrderBy(vm => vm.Variation.Progression.Min)
            .ThenBy(vm => vm.Variation.Progression.Max == null)
            .ThenBy(vm => vm.Variation.Progression.Max)
            .ThenBy(vm => vm.Variation.Name)
            .ToList();
    }

    #endregion
}
