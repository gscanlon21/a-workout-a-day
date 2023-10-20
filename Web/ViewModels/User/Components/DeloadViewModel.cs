namespace Web.ViewModels.User.Components;

/// <summary>
/// Viewmodel for Deload.cshtml
/// </summary>
public class DeloadViewModel
{
    public required bool NeedsDeload { get; set; }
    public required TimeSpan TimeUntilDeload { get; set; }
}
