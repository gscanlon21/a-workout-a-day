using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Web.Extensions;

public static class DisplayExtensions
{
    public static HtmlString? DisplayDescriptionFor<TModel, TProperty>(
        this IHtmlHelper<TModel> helper,
        Expression<Func<TModel, TProperty>> expression)
    {
        if (expression.Body is MemberExpression mExpression)
        {
            var fi = typeof(TModel).GetProperty(mExpression.Member.Name);
            if (fi != null)
            {
                var desc = fi.GetCustomAttribute<DisplayAttribute>(false)?.Description;
                if (desc != null)
                {
                    return new HtmlString(desc);
                }
            }
        }

        return null;
    }
}
