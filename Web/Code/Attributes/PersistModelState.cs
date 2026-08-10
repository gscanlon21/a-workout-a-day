using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Web.Code.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class PersistModelStateAttribute : ActionFilterAttribute
{
    private const string TempDataKey = "_PersistedModelState";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is not Controller controller)
        {
            return;
        }

        if (!controller.TempData.TryGetValue(TempDataKey, out var value) || value == null)
        {
            return;
        }

        var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(value.ToString()!);
        if (errors == null)
        {
            return;
        }

        foreach (var (key, messages) in errors)
        {
            foreach (var message in messages)
            {
                context.ModelState.AddModelError(key, message);
            }
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is not Controller controller)
        {
            return;
        }

        if (context.Result is not RedirectToActionResult)
        {
            return;
        }

        if (context.ModelState.IsValid)
        {
            return;
        }

        var modelStateDict = context.ModelState.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        controller.TempData[TempDataKey] = JsonSerializer.Serialize(modelStateDict);
    }
}