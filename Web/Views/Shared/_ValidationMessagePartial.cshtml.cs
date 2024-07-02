
namespace Web.Views.Shared;

public class _ValidationMessagePartialModel
{
    public bool? WasUpdated { get; set; }
    public string DefaultFailureMessage = "Something went wrong.";
    public string DefaultSuccessMessage = "Your preferences have been saved. Changes will be reflected in the next workout.";
}
