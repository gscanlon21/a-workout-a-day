using Microsoft.AspNetCore.Components;

namespace Hybrid.Pages;

public abstract class RefreshablePageBase : ComponentBase
{
    public static readonly IDictionary<string, NavigationManager> Current = new Dictionary<string, NavigationManager>();

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? RefreshId { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue(nameof(RefreshId), out string? refreshId) && refreshId != null)
        {
            Current.TryAdd(refreshId, NavigationManager);
        }

        return base.SetParametersAsync(parameters);
    }
}