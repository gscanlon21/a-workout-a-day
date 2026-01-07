using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web.Code.TempData;
using Web.Services;

namespace Web.Pages;

public class LoginPageModel : PageModel
{
    private readonly UserRepo _userRepo;
    private readonly CaptchaService _captchaService;

    public LoginPageModel(UserRepo userRepo, CaptchaService captchaService)
    {
        _captchaService = captchaService;
        _userRepo = userRepo;
    }

    [Required, EmailAddress]
    [BindProperty, Display(Name = "Email")]
    public string? Email { get; set; }

    [Required, DataType(DataType.Password)]
    [BindProperty, Display(Name = "Token")]
    public string? Token { get; set; }

    public void OnGet() { }
    public async Task<IActionResult> OnPost([FromForm(Name = "frc-captcha-solution")] string frcCaptchaSolution)
    {
        if (!ModelState.IsValid || _captchaService.VerifyCaptcha(frcCaptchaSolution).Result?.Success == false)
        {
            TempData[TempData_User.FailureMessage] = "Something went wrong!";
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

        return LocalRedirect($"/n/{Uri.EscapeDataString(user.Email)}?token={Uri.EscapeDataString(token)}&client={Client.PWA}");
    }
}
