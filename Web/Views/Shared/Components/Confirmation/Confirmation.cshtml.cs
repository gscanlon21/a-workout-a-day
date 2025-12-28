namespace Web.Views.Shared.Components.Confirmation;

public class ConfirmationViewModel
{
    public required string Token { get; init; } = null!;
    public required Data.Entities.Users.User User { get; init; } = null!;
}
