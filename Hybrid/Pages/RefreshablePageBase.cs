using Microsoft.AspNetCore.Components;

namespace Hybrid.Pages;


public abstract class RefreshablePageBase : ComponentBase
{
    public static RefreshablePageBase? Current;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    protected RefreshablePageBase()
    {
        Current = this;
    }
}