
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Code.Attributes;

public sealed class RequestContext<T> where T : class
{
    public T? Context { get; set; }
    public T RequireContext => Context!;
    public bool HasContext => Context != null;
}

public sealed class SaveParamsFilter<T> : IActionFilter where T : class
{
    private readonly RequestContext<T> _context;

    public SaveParamsFilter(RequestContext<T> context)
    {
        _context = context;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var constructor = typeof(T).GetConstructors().Single();
        var args = constructor.GetParameters().Select(p =>
        {
            if (context.ActionArguments.TryGetValue(p.Name!, out var value))
            {
                return value;
            }

            return p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
        })
        .ToArray();

        _context.Context = (T)constructor.Invoke(args);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class SaveParamsAttribute<T> : TypeFilterAttribute where T : class
{
    public SaveParamsAttribute() : base(typeof(SaveParamsFilter<T>)) { }
}