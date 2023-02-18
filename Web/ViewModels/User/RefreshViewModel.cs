namespace Web.ViewModels.User;

public class RefreshViewModel
{
    public required bool NeedsFunctionalRefresh { get; set; }
    public required TimeSpan TimeUntilFunctionalRefresh { get; set; }

    public required bool NeedsAccessoryRefresh { get; set; }
    public required TimeSpan TimeUntilAccessoryRefresh { get; set; }
}
