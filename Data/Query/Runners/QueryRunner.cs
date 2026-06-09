using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Query.Options;
using Data.Query.Options.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Core.Code.Extensions.EnumerableExtensions;

namespace Data.Query.Runners;

/// <summary>
/// Builds and runs an EF Core query for selecting exercises.
/// </summary>
public class QueryRunner : QueryRunnerBase
{
    public QueryRunner(Section section) : base(section) { }

    /// <summary>
    /// Queries the db for the data.
    /// </summary>
    /// <param name="take">Selects this many variations.</param>
    public override async Task<IList<QueryResults>> Query(IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue)
    {
        // Short-circut when either of these options are set without any data. No results are returned.
        if (ExerciseOptions.ExerciseIds?.Any() == false || ExerciseOptions.VariationIds?.Any() == false)
        {
            return [];
        }

        using var scope = factory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var filteredQuery = CreateFilteredExerciseVariationsQuery(context,
            includePrerequisites: SelectionOptions.IncludePrerequisites,
            includeInstructions: SelectionOptions.IncludeInstructions);

        filteredQuery = Filters.FilterSection(filteredQuery, section);
        filteredQuery = Filters.FilterSkills(filteredQuery, SkillsOptions);
        filteredQuery = Filters.FilterEquipment(filteredQuery, EquipmentOptions.Equipment);
        filteredQuery = Filters.FilterSportsFocus(filteredQuery, SportsOptions.SportsFocus);
        filteredQuery = Filters.FilterExercises(filteredQuery, ExerciseOptions.ExerciseIds);
        filteredQuery = Filters.FilterVariations(filteredQuery, ExerciseOptions.VariationIds);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExerciseFocus);
        filteredQuery = Filters.FilterMuscleMovement(filteredQuery, MuscleMovementOptions.MuscleMovement);
        filteredQuery = Filters.FilterMovementPattern(filteredQuery, MovementPatternOptions.MovementPatterns);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExcludeExerciseFocus, exclude: true);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroupOptions.MuscleGroups.Aggregate(MusculoskeletalSystem.None, (c, n) => c | n), include: true, MuscleGroupOptions.MuscleTarget);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, UserOptions.ExcludeRecoveryMuscle, include: false, UserOptions.ExcludeRecoveryMuscleTarget);

        var queryResults = await filteredQuery.Select(a => new InProgressQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();
        return queryResults.Select(r => new QueryResults(section, r.Exercise, r.Variation, r.UserExercise, r.UserVariation, r.Prerequisites, r.Postrequisites, r.EasierVariation, r.HarderVariation, UserOptions.Intensity))
            .OrderBy(vm => vm.Variation.Progression.Min, NullOrder.NullsFirst)
            .ThenBy(vm => vm.Variation.Progression.Max, NullOrder.NullsLast)
            .ThenBy(vm => vm.Variation.Name)
            .ToList();
    }
}
