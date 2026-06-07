using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web.Code.Attributes.Data;
using Web.Controllers;
using Web.Controllers.Index;

namespace Web.Views.Index;

/// <summary>
/// For CRUD actions.
/// </summary>
public class CreateViewModel
{
    public UserLoginViewModel UserLoginViewModel { get; set; } = new UserLoginViewModel();
    public UserCreateViewModel UserCreateViewModel { get; set; } = new UserCreateViewModel()
    {
        IsNewToFitness = true
    };

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
    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(ViewController.EmailRegex, ErrorMessage = ViewController.EmailRegexError)]
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
    [Required, RegularExpression(ViewController.EmailRegex, ErrorMessage = ViewController.EmailRegexError)]
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
