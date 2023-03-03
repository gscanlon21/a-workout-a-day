namespace Web.ViewModels.Exercise;

/// <summary>
/// Viewmodel for Check.cshtml
/// </summary>
public class CheckViewModel
{
    public IList<string> StretchHasStability { get; init; } = null!;
    public IList<string> Missing100PProgressionRange { get; init; } = null!;
    public IList<string> MissingRepRange { get; init; } = null!;
    public IList<string> MissingProficiencyStrength { get; init; } = null!;
    public IList<string> MissingProficiencyRecovery { get; init; } = null!;
    public IList<string> MissingProficiencyWarmupCooldown { get; init; } = null!;
    public IList<string> EmptyDisabledString { get; init; } = null!;
    public IList<string> MissingExercises { get; init; } = null!;
}
