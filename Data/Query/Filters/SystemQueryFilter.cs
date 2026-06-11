using Core.Models.Exercise;
using Core.Models.Newsletter;
using Microsoft.Extensions.DependencyInjection;
using static Data.Query.Runners.BaseQueryRunner;

namespace Data.Query.Filters;

public class SystemQueryFilter : BaseQueryFilter
{
    public override async Task<List<QueryResults>> Filter(List<InProgressQueryResults> queryResults, IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue)
    {
        return queryResults.Select(r => new QueryResults(Section.None, r.Exercise, r.Variation, r.UserExercise, r.UserVariation, r.Prerequisites, r.Postrequisites, r.EasierVariation, r.HarderVariation, Intensity.None)).ToList();
    }
}
