using Lib.ViewModels.User;

namespace App;

public class AppState
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateOnly? Date { get; set; }
}