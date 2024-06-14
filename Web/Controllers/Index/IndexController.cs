using Core.Consts;
using Core.Models.Options;
using Data;
using Data.Entities.Newsletter;
using Data.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web.Code.TempData;
using Web.Controllers.User;
using Web.Services;
using Web.ViewModels.User;

namespace Web.Controllers.Index;

public class IndexController(CoreContext context, UserRepo userRepo, CaptchaService captchaService, IOptions<SiteSettings> siteSettings) : ViewController()
{

    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Index";

    /// <summary>
    /// Server availability check
    /// </summary>
    [Route("ping")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }

    /// <summary>
    /// Landing page.
    /// </summary>
    [Route("")]
    public IActionResult Index(bool? wasUnsubscribed = null)
    {
        return View("Create", new CreateViewModel()
        {
            WasUnsubscribed = wasUnsubscribed
        });
    }

    [Route(""), HttpPost]
    public async Task<IActionResult> Create(
        [Bind("Email,AcceptedTerms,IsNewToFitness,IExist", Prefix = nameof(UserCreateViewModel))] UserCreateViewModel viewModel,
        [FromForm(Name = "frc-captcha-solution")] string frcCaptchaSolution)
    {
        if (ModelState.IsValid && captchaService.VerifyCaptcha(frcCaptchaSolution).Result?.Success != false)
        {
            var newUser = new Data.Entities.User.User(viewModel.Email, viewModel.AcceptedTerms, viewModel.IsNewToFitness);

            try
            {
                // This will set the Id prop on newUser when changes are saved.
                context.Add(newUser);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException e) when (e.InnerException != null && e.InnerException.Message.Contains("duplicate key"))
            {
                // User may have clicked the back button after personalizing their routine right after signing up.
                return RedirectToAction(nameof(Index), Name);
            }

            // Send an account confirmation email.
            await SendConfirmationEmail(newUser);

            // Need a token for if the user chooses to manage their preferences after signup.
            var token = await userRepo.AddUserToken(newUser, durationDays: 1);
            TempData[TempData_User.SuccessMessage] = "Thank you! Please accept the account confirmation email in your inbox to begin receiving workouts.";
            return RedirectToAction(nameof(UserController.Edit), UserController.Name, new { newUser.Email, token });
        }

        return View(nameof(Create), new CreateViewModel()
        {
            WasSubscribed = false,
            UserCreateViewModel = viewModel
        });
    }

    [Route("login"), HttpPost]
    public async Task<IActionResult> Login(
        [Bind("Email,IExist", Prefix = "UserLoginViewModel")] UserLoginViewModel viewModel,
        [FromForm(Name = "frc-captcha-solution")] string frcCaptchaSolution)
    {
        if (ModelState.IsValid && captchaService.VerifyCaptcha(frcCaptchaSolution).Result?.Success != false)
        {
            // Not going through the normal GetUser route, we don't have a token.
            var unauthenticatedUser = await context.Users
                // TODO email type for transactional or marketing workouts.
                // User has not been sent an email today. Disabling this, we have a captcha now and it's possible someone's email bounces and they need a second email to send.
                //.Where(u => u.LastActive.HasValue || !u.UserEmails.Where(un => un.Subject == NewsletterConsts.SubjectConfirm).Any(d => d.Date == Today))
                //.Where(u => !u.LastActive.HasValue || !u.UserEmails.Where(un => un.Subject == NewsletterConsts.SubjectLogin).Any(d => d.Date == Today))
                .FirstOrDefaultAsync(u => u.Email == viewModel.Email);

            if (unauthenticatedUser != null)
            {
                if (unauthenticatedUser.LastActive.HasValue)
                {
                    await SendLoginEmail(unauthenticatedUser);
                }
                else
                {
                    await SendConfirmationEmail(unauthenticatedUser);
                }
            }

            TempData[TempData_User.SuccessMessage] = "If an account exists for this email, you will receive an email with a link to access your account.";
            return RedirectToAction(nameof(Create), Name);
        }

        return View(nameof(Create), new CreateViewModel()
        {
            WasSubscribed = false,
            UserLoginViewModel = viewModel
        });
    }

    #region Account Emails

    private async Task SendConfirmationEmail(Data.Entities.User.User unauthenticatedUser)
    {
        var token = await userRepo.AddUserToken(unauthenticatedUser, durationDays: 100); // Needs to last at least 3 months by law for unsubscribe link.
        var userNewsletter = new UserEmail(unauthenticatedUser)
        {
            Subject = EmailConsts.SubjectConfirm,
            Body = $@"
This is an account confirmation email for your newly created <a href='{siteSettings.Value.WebLink}'>{siteSettings.Value.Name}</a> account. If this was not you, you can safely ignore this email.
<br><br>
<a rel='noopener noreferrer' target='_blank' href='{Url.ActionLink(nameof(UserController.IAmStillHere), UserController.Name, new { unauthenticatedUser.Email, token })}'>Confirm my account</a>
<br><br>
<hr style='margin-block:1ex;'>
<a rel='noopener noreferrer' target='_blank' href='{Url.ActionLink(nameof(UserController.Delete), UserController.Name, new { unauthenticatedUser.Email, token })}'>Unsubscribe</a>
<hr style='margin-block:1ex;'>
<span><a href='{Url.PageLink("/Terms")}' rel='noopener noreferrer' target='_blank'>Terms of Use</a> | <a href='{Url.PageLink("/Privacy")}' rel='noopener noreferrer' target='_blank'>Privacy</a></span>
<hr style='margin-block:1ex;'>
<span><a href='mailto:help@{siteSettings.Value.Domain}' rel='noopener noreferrer' target='_blank'>Contact Us</a> | <a href='{siteSettings.Value.Source}' rel='noopener noreferrer' target='_blank'>Source</a></span>
",
        };

        context.UserEmails.Add(userNewsletter);
        await context.SaveChangesAsync();
    }

    private async Task SendLoginEmail(Data.Entities.User.User unauthenticatedUser)
    {
        var token = await userRepo.AddUserToken(unauthenticatedUser, durationDays: 100); // Needs to last at least 3 months by law for unsubscribe link.
        var userNewsletter = new UserEmail(unauthenticatedUser)
        {
            Subject = EmailConsts.SubjectLogin,
            Body = $@"
Access to login to your <a href='{siteSettings.Value.WebLink}'>{siteSettings.Value.Name}</a> account was recently requested. If this was not you, you can safely ignore this email.
<br><br>
<a rel='noopener noreferrer' target='_blank' href='{Url.ActionLink(nameof(UserController.IAmStillHere), UserController.Name, new { unauthenticatedUser.Email, token })}'>Login to my account</a>
<br><br>
<hr style='margin-block:1ex;'>
<a rel='noopener noreferrer' target='_blank' href='{Url.ActionLink(nameof(UserController.Delete), UserController.Name, new { unauthenticatedUser.Email, token })}'>Unsubscribe</a>
<hr style='margin-block:1ex;'>
<span><a href='{Url.PageLink("/Terms")}' rel='noopener noreferrer' target='_blank'>Terms of Use</a> | <a href='{Url.PageLink("/Privacy")}' rel='noopener noreferrer' target='_blank'>Privacy</a></span>
<hr style='margin-block:1ex;'>
<span><a href='mailto:help@{siteSettings.Value.Domain}' rel='noopener noreferrer' target='_blank'>Contact Us</a> | <a href='{siteSettings.Value.Source}' rel='noopener noreferrer' target='_blank'>Source</a></span>
",
        };

        context.UserEmails.Add(userNewsletter);
        await context.SaveChangesAsync();
    }

    #endregion
    #region User Validation

    /// <summary>
    /// Validation route for whether a user already exists in the database
    /// </summary>
    [AllowAnonymous, Route("availability")]
    public async Task<JsonResult> IsUserAvailable([Bind(Prefix = nameof(UserCreateViewModel))] UserCreateViewModel viewModel)
    {
        var email = viewModel.Email.Trim();

        // Don't let users signup as our domain.
        if (email.Contains(siteSettings.Value.Domain, StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult("Invalid email.");
        }

        // The same user is already signed up.
        if (await context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, email)))
        {
            return new JsonResult("Invalid email.");
        }

        return new JsonResult(true);
    }

    #endregion
}
