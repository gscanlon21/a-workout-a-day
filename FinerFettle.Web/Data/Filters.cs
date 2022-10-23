using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.Data
{
    public static class Filters
    {
        public interface IQueryFiltersSportsFocus
        {
            Variation Variation { get; }
        }

        public static IQueryable<T> FilterSportsFocus<T>(IQueryable<T> query, SportsFocus? sportsFocus) where T : IQueryFiltersSportsFocus
        {
            if (sportsFocus != null)
            {
                return query.Where(q => q.Variation.SportsFocus.HasFlag(sportsFocus));
            }

            return query;
        }
    }
}
