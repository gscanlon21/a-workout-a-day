namespace Web.Views.Shared.Components.Deload;

public class DeloadViewModel
{
    public required bool NeedsDeload { get; set; }
    public required TimeSpan TimeUntilDeload { get; set; }
}
