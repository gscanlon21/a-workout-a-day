namespace Web.ViewModels.User;

public class TokenViewModel
{
    public Data.Entities.User.User User { get; set; } = null!;
    public string Token { get; set; } = null!;
}
