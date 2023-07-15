using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web.Code.Attributes.Data;
using Web.Controllers.Index;

namespace Web.ViewModels.User;

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

    public UserCreateViewModel(Lib.ViewModels.User.UserViewModel user, string token)
    {
        Email = user.Email;
        AcceptedTerms = user.AcceptedTerms;
        IsNewToFitness = user.IsNewToFitness;
        Token = token;
    }

    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(EmailRegex, ErrorMessage = EmailRegexError)]
    [Remote(nameof(IndexController.IsUserAvailable), IndexController.Name)]
    [Display(Name = "Email", Description = "We respect your privacy and sanity.")]
    public string Email { get; init; } = null!;

    public string? Token { get; init; }

    [Required, MustBeTrue]
    [Display(Description = "You must be at least 13 years old.")]
    public bool AcceptedTerms { get; init; }

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplifies workouts to just the core movements.")]
    public bool IsNewToFitness { get; init; }

    /// <summary>
    /// Anti-bot honeypot.
    /// </summary>
    [Required, MustBeTrue(DisableClientSideValidation = true, ErrorMessage = "Bot, I am your father.")]
    [Display(Description = "")]
    public bool IExist { get; init; }
}
