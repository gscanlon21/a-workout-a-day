namespace Web.ViewModels.User.Components;

/// <summary>
/// Viewmodel for Gmail.cshtml
/// </summary>
public class GmailViewModel
{
    public required Data.Entities.User.User User { get; init; } = null!;
}
