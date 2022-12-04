using System.Linq.Expressions;

namespace Web.Extensions;

public static class NameOf<TSource>
{
    public static string Full(Expression<Func<TSource, object>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
        }

        var result = memberExpression?.ToString() ?? "";
        result = result[(result.IndexOf('.') + 1)..];

        return result;
    }

    public static string Full(string sourceFieldName, Expression<Func<TSource, object>> expression)
    {
        var result = Full(expression);
        result = string.IsNullOrEmpty(sourceFieldName) ? result : sourceFieldName + "." + result;
        return result;
    }
}