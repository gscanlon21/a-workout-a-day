using Core.Models.Newsletter;
using Data.Entities.Equipment;
using Data.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Data.Models.Newsletter;

/// <summary>
/// Viewmodel for Footer.cshtml
/// </summary>
public class FooterModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public FooterModel(UserNewsletterModel user)
    {
        User = user;
        Verbosity = user.EmailVerbosity;
    }

    public UserNewsletterModel User { get; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; private init; }

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    [UIHint(nameof(Equipment)), Display(Name = "Equipment")]
    public EquipmentModel AllEquipment { get; init; } = null!;
}
