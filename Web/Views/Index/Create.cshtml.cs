using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web.Code.Attributes.Data;
using Web.Controllers.Index;

namespace Web.Views.Index;


/// <summary>
/// For CRUD actions
/// </summary>
public class CreateViewModel
{
    public const string EmailRegex = @"\s*\S+@\S+\.\S+\s*";
    public const string EmailRegexError = "Please enter a valid email address.";

    public CreateViewModel()
    {
        UserCreateViewModel = new UserCreateViewModel()
        {
            IsNewToFitness = true
        };
    }

    public UserCreateViewModel UserCreateViewModel { get; set; } = new UserCreateViewModel();
    public UserLoginViewModel UserLoginViewModel { get; set; } = new UserLoginViewModel();

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
}


/// <summary>
/// For CRUD actions
/// </summary>
public class UserCreateViewModel
{
    public const string EmailRegex = @"\s*\S+@\S+\.\S+\s*";
    public const string EmailRegexError = "Please enter a valid email address.";

    public UserCreateViewModel()
    {
        IsNewToFitness = true;
    }

    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(EmailRegex, ErrorMessage = EmailRegexError)]
    [Remote(nameof(IndexController.IsUserAvailable), IndexController.Name)]
    [Display(Name = "Email", Description = "We respect your privacy and sanity.")]
    public string Email { get; init; } = null!;

    public string? Token { get; init; }

    [Required, MustBeTrue]
    [Display(Description = "You must be at least 18 years old.")]
    public bool AcceptedTerms { get; init; }

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplify your workouts.")]
    public bool IsNewToFitness { get; init; }

    /// <summary>
    /// Anti-bot honeypot.
    /// </summary>
    [Required, MustBeTrue(DisableClientSideValidation = true, ErrorMessage = "Bot, I am your father.")]
    [Display(Description = "")]
    public bool IExist { get; init; }
}


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
