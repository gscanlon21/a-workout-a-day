namespace Web.ViewModels.User;

public class DeloadViewModel
{
    public required bool NeedsDeload { get; set; }
    public required TimeSpan TimeUntilDeload { get; set; }
}
