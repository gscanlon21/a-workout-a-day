using FinerFettle.Web.Entities.Exercise;
using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace FinerFettle.Web.Data;

public class ExerciseQueryBuilder
{
    public enum OrderByEnum
    {
        None,
        Progression
    }

    public record QueryResults(
        User? User, 
        Exercise Exercise, 
        Variation Variation, 
        ExerciseVariation ExerciseVariation, 
        UserExercise? UserExercise, 
        UserExerciseVariation? UserExerciseVariation,
        UserVariation? UserVariation, 
        IntensityLevel? IntensityLevel,
        bool FirstGoAround
    );

    [DebuggerDisplay("{Variation}")]
    public class InProgressQueryResults : 
        IQueryFiltersSportsFocus, 
        IQueryFiltersExerciseType, 
        IQueryFiltersIntensityLevel,
        IQueryFiltersMuscleContractions,
        IQueryFiltersOnlyWeights,
        IQueryFiltersUnilateral,
        IQueryFiltersEquipmentIds,
        IQueryFiltersRecoveryMuscle,
        IQueryFiltersMuscleGroupMuscle,
        IQueryFiltersShowCore
    {
        public Exercise Exercise { get; init; } = null!;
        public Variation Variation { get; init; } = null!;
        public ExerciseVariation ExerciseVariation { get; init; } = null!;
        public UserExercise? UserExercise { get; init; }
        public UserExerciseVariation? UserExerciseVariation { get; init; }
        public UserVariation? UserVariation { get; init; }
        public IntensityLevel? IntensityLevel { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
    }

    private readonly CoreContext Context;
    private readonly bool IgnoreGlobalQueryFilters = false;

    private User? User;
    private ExerciseType? ExerciseType;
    private MuscleGroups? RecoveryMuscle;
    private MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    private MuscleGroups? IncludeMuscle;
    private MuscleGroups? ExcludeMuscle;
    private MuscleGroups MuscleGroups;
    private bool? PrefersWeights;
    private bool? OnlyWeights;
    private bool? IncludeBonus;
    private MuscleContractions? MuscleContractions;
    private IntensityLevel? IntensityLevel;
    private OrderByEnum OrderBy = OrderByEnum.None;
    private SportsFocus? SportsFocus;
    private int Repeat = 1;
    private int? TakeOut;
    private int? AtLeastXUniqueMusclesPerExercise;
    private bool DoCapAtProficiency = false;
    private bool UniqueMuscles = true;
    private bool? Unilateral = null;
    private IEnumerable<int>? EquipmentIds;
    private IEnumerable<int>? ExerciseExclusions;

    public ExerciseQueryBuilder(CoreContext context, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
    }

    /// <summary>
    /// Filter exercises down to the specified type
    /// </summary>
    public ExerciseQueryBuilder WithExerciseType(ExerciseType exerciseType)
    {
        ExerciseType = exerciseType;
        return this;
    }

    /// <summary>
    /// If true, prefer weighted variations over bodyweight variations.
    /// If false, only show bodyweight variations.
    /// If null, show both weighted and bodyweight variations with equal precedence.
    /// </summary>
    public ExerciseQueryBuilder WithPrefersWeights(bool? prefersWeights)
    {
        PrefersWeights = prefersWeights;
        return this;
    }

    public ExerciseQueryBuilder WithOnlyWeights(bool? onlyWeights)
    {
        OnlyWeights = onlyWeights;
        return this;
    }

    public ExerciseQueryBuilder WithIncludeBonus(bool? includeBonus)
    {
        IncludeBonus = includeBonus;
        return this;
    }

    /// <summary>
    /// Return this many exercise variations
    /// </summary>
    public ExerciseQueryBuilder Take(int @out)
    {
        TakeOut = @out;
        return this;
    }

    /// <summary>
    /// Don't choose variations where the exercise min progression is greater than the exercise proficiency level.
    /// For things like warmups--rather have regular pushups over one-hand pushups.
    /// </summary>
    public ExerciseQueryBuilder CapAtProficiency(bool doCap)
    {
        DoCapAtProficiency = doCap;
        return this;
    }

    /// <summary>
    /// Filter exercises down to unilateral variations
    /// </summary>
    public ExerciseQueryBuilder IsUnilateral(bool isUnilateral)
    {
        Unilateral = isUnilateral;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle contractions
    /// </summary>
    public ExerciseQueryBuilder WithMuscleContractions(MuscleContractions muscleContractions)
    {
        MuscleContractions = muscleContractions;
        return this;
    }

    /// <summary>
    /// Filter variations down to the user's progressions
    /// </summary>
    public ExerciseQueryBuilder WithUser(User? user)
    {
        User = user;
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment
    /// </summary>
    public ExerciseQueryBuilder WithEquipment(IEnumerable<int> equipmentIds)
    {
        EquipmentIds = equipmentIds;
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment
    /// </summary>
    public ExerciseQueryBuilder WithExcludeExercises(IEnumerable<int> equipmentIds)
    {
        ExerciseExclusions = equipmentIds;
        return this;
    }

    /// <summary>
    /// Filter exercises down to these muscle groups.
    /// 
    /// Does not do anything if AtLeastXUniqueMusclesPerExercise is unset.
    /// </summary>
    public ExerciseQueryBuilder WithMuscleGroups(MuscleGroups muscleGroups)
    {
        MuscleGroups = muscleGroups;
        return this;
    }

    /// <summary>
    /// Already worked muscle groups
    /// </summary>
    public ExerciseQueryBuilder WithAlreadyWorkedMuscles(MuscleGroups muscleGroups)
    {
        MusclesAlreadyWorked = muscleGroups;
        return this;
    }

    /// <summary>
    /// Filter exercises down to this intensity level
    /// </summary>
    public ExerciseQueryBuilder WithIntensityLevel(IntensityLevel intensityLevel)
    {
        IntensityLevel = intensityLevel;
        return this;
    }

    /// <summary>
    /// Order the final results
    /// </summary>
    public ExerciseQueryBuilder WithOrderBy(OrderByEnum orderBy)
    {
        OrderBy = orderBy;
        return this;
    }

    /// <summary>
    /// Filter exercises to where each exercise choosen works X unique muscle groups
    /// </summary>
    public ExerciseQueryBuilder WithAtLeastXUniqueMusclesPerExercise(int x, bool uniqueMuscles = true, int repeat = 1)
    {
        AtLeastXUniqueMusclesPerExercise = x;
        UniqueMuscles = uniqueMuscles;
        Repeat = repeat;
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this spoirt
    /// </summary>
    public ExerciseQueryBuilder WithSportsFocus(SportsFocus sportsFocus)
    {
        SportsFocus = sportsFocus;
        return this;
    }

    /// <summary>
    /// Filer out exercises that touch on an injured muscle
    /// </summary>
    public ExerciseQueryBuilder WithRecoveryMuscle(MuscleGroups recoveryMuscle)
    {
        RecoveryMuscle = recoveryMuscle;
        return this;
    }

    /// <summary>
    /// Filer out exercises that touch on an injured muscle
    /// </summary>
    public ExerciseQueryBuilder WithIncludeMuscle(MuscleGroups? includeMuscle)
    {
        IncludeMuscle = includeMuscle;
        return this;
    }

    /// <summary>
    /// Filer out exercises that touch on an injured muscle
    /// </summary>
    public ExerciseQueryBuilder WithExcludeMuscle(MuscleGroups? excludeMuscle)
    {
        ExcludeMuscle = excludeMuscle;
        return this;
    }

    /// <summary>
    /// Queries the db for the data
    /// </summary>
    public async Task<IList<QueryResults>> Query()
    {
        var eligibleExercisesQuery = Context.Exercises
            .Include(e => e.Prerequisites) // TODO Only necessary for the /exercises list, not the newsletter
                .ThenInclude(p => p.PrerequisiteExercise)
            .Select(i => new {
                Exercise = i,
                UserExercise = i.UserExercises.FirstOrDefault(ue => ue.User == User)
            })
            // Don't grab exercises that the user wants to ignore
            .Where(i => i.UserExercise == null || !i.UserExercise.Ignore)
            // Only show these exercises if the user has completed the previous reqs
            .Where(i => i.Exercise.Prerequisites
                    .Select(r => new {
                        r.PrerequisiteExercise.Proficiency, 
                        UserExercise = r.PrerequisiteExercise.UserExercises.FirstOrDefault(up => up.User == User) 
                    })
                    .All(p => User == null 
                        || (/* Require the prerequisites show first */ p.UserExercise != null
                            && (p.UserExercise.Ignore || p.UserExercise.Progression >= p.Proficiency))
                    )
            );

        var baseQuery = Context.Variations
            .AsNoTracking() // Don't update any entity
            .Include(i => i.Intensities)
            .Include(i => i.EquipmentGroups.Where(eg => eg.Parent == null).Where(eg => PrefersWeights == false ? !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight)) : true))
                // To display the equipment required for the exercise in the newsletter
                .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Include(i => i.EquipmentGroups.Where(eg => eg.Parent == null).Where(eg => PrefersWeights == false ? !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight)) : true))
                .ThenInclude(eg => eg.Children)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Join(Context.ExerciseVariations, o => o.Id, i => i.Variation.Id, (o, i) => new { 
                Variation = o, 
                ExerciseVariation = i 
            })
            .Join(eligibleExercisesQuery, o => o.ExerciseVariation.Exercise.Id, i => i.Exercise.Id, (o, i) => new { 
                o.Variation, 
                o.ExerciseVariation, 
                i.Exercise, 
                i.UserExercise
            })
            .Where(vm => ExerciseExclusions == null ? true : !ExerciseExclusions.Contains(vm.Exercise.Id))
            .Where(vm => DoCapAtProficiency ? (vm.ExerciseVariation.Progression.Min == null || vm.ExerciseVariation.Progression.Min <= vm.ExerciseVariation.Exercise.Proficiency) : true)
            .Select(a => new InProgressQueryResults() { 
                UserExercise = a.UserExercise,
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.User == User),
                UserExerciseVariation = a.ExerciseVariation.UserExerciseVariations.FirstOrDefault(uv => uv.User == User),
                Exercise = a.Exercise,
                Variation = a.Variation,
                ExerciseVariation = a.ExerciseVariation,
                IntensityLevel = IntensityLevel,
                IsMaxProgressionInRange = User != null && (
                    a.ExerciseVariation.Progression.Max == null
                    // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                    || (a.UserExercise == null && (UserExercise.MinUserProgression < a.ExerciseVariation.Progression.Max))
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise != null && (a.UserExercise!.Progression < a.ExerciseVariation.Progression.Max))
                )
            });

        if (IgnoreGlobalQueryFilters)
        {
            baseQuery = baseQuery.IgnoreQueryFilters();
        }

        if (User != null)
        {
            baseQuery = baseQuery.Where(i => i.ExerciseVariation.Progression.Min == null
                            // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                            || (i.UserExercise == null && (UserExercise.MinUserProgression >= i.ExerciseVariation.Progression.Min))
                            // Compare the exercise's progression range with the user's exercise progression
                            || (i.UserExercise != null && (i.UserExercise!.Progression >= i.ExerciseVariation.Progression.Min)));

            baseQuery = baseQuery.Where(i => 
                            // User owns at least one equipment in at least one of the optional equipment groups
                            i.Variation.EquipmentGroups.Any(eg => !eg.Equipment.Any())
                            || i.Variation.EquipmentGroups.Where(eg => eg.Equipment.Any()).Any(peg => 
                                peg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)) 
                                && (
                                    !peg.Children.Any() 
                                    || peg.Children.Any(ceg => ceg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                                )
                            )
                        );
        }

        baseQuery = Filters.FilterMuscleGroup(baseQuery, IncludeMuscle, include: true);
        baseQuery = Filters.FilterMuscleGroup(baseQuery, ExcludeMuscle, include: false);
        baseQuery = Filters.FilterEquipmentIds(baseQuery, EquipmentIds);
        baseQuery = Filters.FilterEquipmentIds(baseQuery, EquipmentIds);
        baseQuery = Filters.FilterRecoveryMuscle(baseQuery, RecoveryMuscle);
        baseQuery = Filters.FilterSportsFocus(baseQuery, SportsFocus);
        baseQuery = Filters.FilterIncludeBonus(baseQuery, IncludeBonus);
        baseQuery = Filters.FilterMuscleContractions(baseQuery, MuscleContractions);
        baseQuery = Filters.FilterExerciseType(baseQuery, ExerciseType);
        baseQuery = Filters.FilterIntensityLevel(baseQuery, IntensityLevel);
        baseQuery = Filters.FilterOnlyWeights(baseQuery, OnlyWeights);
        baseQuery = Filters.FilterIsUnilateral(baseQuery, Unilateral);

        // TODO? Don't show an exercises weighted equipment groups if the user is not over the exercise's proficiency level?
        if (PrefersWeights == false)
        {
            baseQuery = baseQuery.Where(vm => vm.Variation.EquipmentGroups.Any(eg => !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight))));
        }

        var queryResults = (await baseQuery.ToListAsync()).AsEnumerable();

        if (User != null)
        {
            // Try choosing variations that have a max progression above the user's progression. Fallback to an easier variation if one does not exist.
            queryResults = queryResults.GroupBy(i => new { i.Exercise.Id })
                                .Select(g => new
                                {
                                    g.Key,
                                    // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                    Variations = g.Where(a => a.IsMaxProgressionInRange).NullIfEmpty() ?? g.Where(a => !a.IsMaxProgressionInRange).OrderByDescending(a => a.ExerciseVariation.Progression.GetMaxOrDefault).Take(1)
                                })
                                .SelectMany(g => g.Variations);
        }

        // OrderBy must come after query or you get duplicates
        // Show exercises that the user has rarely seen
        var orderedResults = queryResults.OrderBy(a => a.UserExercise == null ? DateOnly.MinValue : a.UserExercise.LastSeen);

        if (PrefersWeights == true)
        {
            // User prefers weighted variations, order those next.
            // TODO? What about a variation that has two equipment groups, one bodyweight and one weighted? Is the exercise weighted?
            // TODO? Should we only look at equipment groups that the user owns when ordering by IsWeight?
            orderedResults = orderedResults.ThenByDescending(a => a.Variation.EquipmentGroups.Any(eg => eg.IsWeight));
        }

        orderedResults = orderedResults
            // Show variations that the user has rarely seen
            .ThenBy(a => a.UserExerciseVariation == null ? DateOnly.MinValue : a.UserExerciseVariation.LastSeen)
            // Mostly for the demo, show mostly random exercises
            .ThenBy(a => Guid.NewGuid());

        var finalResults = new List<QueryResults>();
        if (AtLeastXUniqueMusclesPerExercise != null)
        {
            if (MuscleGroups == MuscleGroups.None)
            {
                throw new ArgumentNullException(nameof(MuscleGroups));
            }

            if (AtLeastXUniqueMusclesPerExercise > BitOperations.PopCount((ulong)MuscleGroups))
            {
                throw new ArgumentOutOfRangeException(nameof(AtLeastXUniqueMusclesPerExercise));
            }

            if (UniqueMuscles)
            {
                while (AtLeastXUniqueMusclesPerExercise > 1)
                {
                    foreach (var exercise in orderedResults)
                    {
                        if (finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                        {
                            continue;
                        }

                        var primaryMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => finalResults.Sum(r => r.Variation.PrimaryMuscles.HasFlag(v) ? 1 : 0));
                        var allMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => finalResults.Sum(r => r.Variation.AllMuscles.HasFlag(v) ? 1 : 0));
                        // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                        if (BitOperations.PopCount((ulong)MuscleGroups.UnsetFlag32(exercise.Variation.PrimaryMuscles.UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= Repeat).Aggregate((MuscleGroups)0, (curr, n) => curr | n.Key)))) <= (BitOperations.PopCount((ulong)MuscleGroups) - AtLeastXUniqueMusclesPerExercise)
                            && BitOperations.PopCount((ulong)exercise.Variation.PrimaryMuscles.UnsetFlag32(allMusclesWorked.Where(d => d.Value >= 2).Aggregate((MuscleGroups)0, (curr, n) => curr | n.Key))) > 0)
                        {
                            var firstGoAround = BitOperations.PopCount((ulong)MuscleGroups.UnsetFlag32(exercise.Variation.PrimaryMuscles.UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate(MusclesAlreadyWorked, (curr, n) => curr | n.Key)))) <= (BitOperations.PopCount((ulong)MuscleGroups) - 1);
                            finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, IntensityLevel, firstGoAround));
                        }
                    }

                    // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
                    // Choose a 3-muscle group compound exercise or a 2-muscle group compound exercise and then an isolation exercise.
                    AtLeastXUniqueMusclesPerExercise--;
                }

                foreach (var exercise in orderedResults)
                {
                    if (finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                    {
                        continue;
                    }

                    var musclesWorkedSoFar = finalResults.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Variation.PrimaryMuscles);
                    // Grab any muscle groups we missed in the previous loops. Include isolation exercises here
                    if (exercise.Variation.PrimaryMuscles.UnsetFlag32(musclesWorkedSoFar).HasAnyFlag32(MuscleGroups))
                    {
                        finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, IntensityLevel, true));
                    }
                }
            }
            else
            {
                foreach (var exercise in orderedResults)
                {
                    // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                    if (BitOperations.PopCount((ulong)MuscleGroups.UnsetFlag32(exercise.Variation.PrimaryMuscles)) <= (BitOperations.PopCount((ulong)MuscleGroups) - AtLeastXUniqueMusclesPerExercise))
                    {
                        finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, IntensityLevel, true));
                    }
                }
            }
        } 
        else
        {
            finalResults = orderedResults.Select(a => new QueryResults(User, a.Exercise, a.Variation, a.ExerciseVariation, a.UserExercise, a.UserExerciseVariation, a.UserVariation, IntensityLevel, true)).ToList();
        }

        if (TakeOut != null)
        {
            finalResults = finalResults.Take(TakeOut.Value).ToList();
        }

        finalResults = OrderBy switch
        {
            OrderByEnum.Progression => finalResults.OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                                    .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                                    .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
                                    .ToList(),
            _ => finalResults.OrderBy(vm => vm.Exercise.Name)
                                    .ThenBy(vm => vm.Variation.Name)
                                    .ToList(),
        };

        return finalResults;
    }
}
