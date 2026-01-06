using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.Code.TempData;

namespace Web.Pages;

public class LoginPageModel : PageModel
{
    private readonly UserRepo _userRepo;

    public LoginPageModel(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    [Required, EmailAddress]
    [BindProperty, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [BindProperty, Display(Name = "Token")]
    public string Token { get; set; } = string.Empty;

    public void OnGet() { }
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userRepo.GetUser(Email, Token);
        if (user == null)
        {
            TempData[TempData_User.FailureMessage] = "Invalid user!";
            return Page();
        }

        var token = await _userRepo.GetPersistentToken(user);
        if (token == null)
        {
            TempData[TempData_User.FailureMessage] = "Missing token!";
            return Page();
        }

        return LocalRedirect($"/n/{user.Email}?token={token}");
    }
}
