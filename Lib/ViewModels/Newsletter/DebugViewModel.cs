﻿using Core.Models.Newsletter;
using Lib.ViewModels.Equipment;
using Lib.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Debug.cshtml
/// </summary>
public class DebugViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DebugViewModel(User.UserViewModel user, string token)
    {
        //User = new UserNewsletterViewModel(user, token);
        Verbosity = user.EmailVerbosity;
    }

    public UserNewsletterViewModel User { get; } = null!;

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; init; }

    public required IList<ExerciseViewModel> DebugExercises { get; init; }

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    [UIHint(nameof(Equipment.EquipmentViewModel))]
    public EquipmentViewModel AllEquipment { get; init; } = null!;
}
