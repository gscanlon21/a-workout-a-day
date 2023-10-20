using Core.Models.Newsletter;

namespace Web.Code.RouteConstraints
{
    public class SectionRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// The constraint name for use in routes like /{sect:section}/.
        /// </summary>
        public const string Name = "section";

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // Retrieve the candidate value
            var candidate = values[routeKey]?.ToString();
            // Attempt to parse the candidate to the required Enum type, and return the result
            return Enum.TryParse(candidate, out Section _);
        }
    }
}
