using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web.Code.Attributes.Data;
using Web.Controllers.Index;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserLoginViewModel
{
    public const string EmailRegex = @"\s*\S+@\S+\.\S+\s*";
    public const string EmailRegexError = "Please enter a valid email address.";

    public UserLoginViewModel()
    {
    }

    public UserLoginViewModel(Lib.ViewModels.User.UserViewModel user, string token)
    {
        Email = user.Email;
        Token = token;
    }

    /// <summary>
    /// If null, user has not yet tried to subscribe.
    /// If true, user has successfully subscribed.
    /// If false, user failed to subscribe.
    /// </summary>
    public bool? WasSubscribed { get; set; }

    /// <summary>
    /// If null, user has not yet tried to unsubscribe.
    /// If true, user has successfully unsubscribed.
    /// If false, user failed to unsubscribe.
    /// </summary>
    public bool? WasUnsubscribed { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(EmailRegex, ErrorMessage = EmailRegexError)]
    [Display(Name = "Email", Description = "")]
    public string Email { get; init; } = null!;

    public string? Token { get; init; }

    /// <summary>
    /// Anti-bot honeypot.
    /// </summary>
    [Required, MustBeTrue(DisableClientSideValidation = true, ErrorMessage = "Bot, I am your father.")]
    [Display(Description = "")]
    public bool IExist { get; init; }
}
