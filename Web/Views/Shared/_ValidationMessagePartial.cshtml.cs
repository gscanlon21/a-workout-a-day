namespace Web.Views.Shared;

public class ValidationMessagePartialModel
{
    public bool? WasUpdated { get; init; }
    public bool GoBackOnSave { get; init; }

    public const string DefaultFailureMessage = "Something went wrong.";
    public const string DefaultSuccessMessage = "Your preferences have been saved. Changes will be reflected soon.";
}
