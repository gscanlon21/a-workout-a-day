using Microsoft.Extensions.DependencyInjection;
using static Data.Query.Runners.BaseQueryRunner;

namespace Data.Query.Filters;

public abstract class BaseQueryFilter
{
    public abstract Task<List<QueryResults>> Filter(List<InProgressQueryResults> queryResults, IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue);
}
