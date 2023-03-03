namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for Deload.cshtml
/// </summary>
public class DeloadViewModel
{
    public required bool NeedsDeload { get; set; }
    public required TimeSpan TimeUntilDeload { get; set; }
}
