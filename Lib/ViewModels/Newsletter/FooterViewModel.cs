using Core.Models.Newsletter;
using Lib.ViewModels.Equipment;
using Lib.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Footer.cshtml
/// </summary>
public class FooterViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public FooterViewModel(UserNewsletterViewModel user)
    {
        User = user;
        Verbosity = user.EmailVerbosity;
    }

    public UserNewsletterViewModel User { get; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; init; }

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    [UIHint(nameof(Equipment.EquipmentViewModel)), Display(Name = "Equipment")]
    public EquipmentViewModel AllEquipment { get; init; } = null!;
}
