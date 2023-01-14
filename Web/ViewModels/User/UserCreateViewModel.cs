using Web.Code.Attributes.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserCreateViewModel
{
    public UserCreateViewModel() 
    {
        IsNewToFitness = true;
    }

    public UserCreateViewModel(Entities.User.User user, string token)
    {
        Email = user.Email;
        AcceptedTerms = user.AcceptedTerms;
        IsNewToFitness = user.IsNewToFitness;
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
    [Required, RegularExpression(@"\s*\S+@\S+\.\S+\s*", ErrorMessage = "Invalid email.")]
    [Remote(nameof(Controllers.UserValidationController.IsUserAvailable), Controllers.UserValidationController.Name, ErrorMessage = "Invalid email. Manage your preferences from the previous newsletter.")]
    [Display(Name = "Email", Description = "We respect your privacy and sanity.")]
    public string Email { get; init; } = null!;

    public string? Token { get; init; }

    [Required, MustBeTrue]
    [Display(Description = "You must be at least 13 years old.")]
    public bool AcceptedTerms { get; init; }

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplifies workouts to help make working out a habit.")]
    public bool IsNewToFitness { get; init; }

    /// <summary>
    /// Anti-bot honeypot
    /// </summary>
    [Required, MustBeTrue(DisableClientSideValidation = true, ErrorMessage = "Bot, I am your father.")]
    [Display(Description = "If you're reading this, you're human.")]
    public bool IExist { get; init; }
}
