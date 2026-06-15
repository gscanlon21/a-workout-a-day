using Data.Query.Options;
using Data.Query.Options.Users;
using Microsoft.Extensions.DependencyInjection;
using static Core.Code.Extensions.EnumerableExtensions;
using static Data.Query.Runners.BaseQueryRunner;

namespace Data.Query.Filters;

public class ExerciseQueryFilter : BaseQueryFilter
{
    protected readonly Core.Models.Newsletter.Section section;

    public ExerciseQueryFilter(Core.Models.Newsletter.Section sec)
    {
        section = sec;
    }

    public required UserOptions UserOptions { protected get; init; }
    public required ExerciseOptions ExerciseOptions { protected get; init; }
    public required SelectionOptions SelectionOptions { protected get; init; }

    public override async Task<List<QueryResults>> Filter(List<InProgressQueryResults> queryResults, IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue)
    {
        return queryResults.Select(r => new QueryResults(section, r.Exercise, r.Variation, r.UserExercise, r.UserVariation, r.Prerequisites, r.Postrequisites, r.EasierVariation, r.HarderVariation, UserOptions.Intensity))
            .OrderBy(vm => vm.Variation.Progression.Min, NullOrder.NullsFirst)
            .ThenBy(vm => vm.Variation.Progression.Max, NullOrder.NullsLast)
            .ThenBy(vm => vm.Variation.Name)
            .ToList();
    }
}
