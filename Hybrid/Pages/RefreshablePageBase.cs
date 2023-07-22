using Microsoft.AspNetCore.Components;

namespace Hybrid.Pages;

public abstract class RefreshablePageBase : ComponentBase
{
    public static readonly IDictionary<string, NavigationManager> Current = new Dictionary<string, NavigationManager>();

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? RefreshId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (RefreshId != null)
        {
            Current.TryAdd(RefreshId, NavigationManager);
        }
    }
}