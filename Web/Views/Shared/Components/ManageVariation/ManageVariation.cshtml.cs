﻿using Core.Consts;
using Core.Dtos.Newsletter;
using Core.Models.Newsletter;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Web.Views.User;

namespace Web.Views.Shared.Components.ManageVariation;


/// <summary>
/// For CRUD actions
/// </summary>
public class ManageVariationViewModel
{
    [ValidateNever]
    public required ManageExerciseVariationDto.Params Parameters { get; init; }

    [ValidateNever]
    public required Data.Entities.User.User User { get; init; }

    [ValidateNever]
    [Display(Name = "Refreshes After", Description = "Refresh this variation—the next workout will try and select a new exercise variation if available.")]
    public required Data.Entities.User.UserVariation UserVariation { get; init; }

    [ValidateNever]
    [Display(Name = "Variation", Description = "Ignore this variation for just this section.")]
    public required ExerciseVariationDto ExerciseVariation { get; init; } = null!;

    public Verbosity VariationVerbosity => Verbosity.Instructions | Verbosity.Images;

    [Required, Range(UserConsts.UserWeightMin, UserConsts.UserWeightMax)]
    [Display(Name = "How much weight are you able to lift?")]
    public int Weight { get; init; }

    [Required, Range(UserConsts.UserSetsMin, UserConsts.UserSetsMax)]
    [Display(Name = "How many sets did you do?")]
    public int Sets { get; init; }

    [Required, Range(UserConsts.UserRepsMin, UserConsts.UserRepsMax)]
    [Display(Name = "How many reps/secs did you do?")]
    public int Reps { get; init; }

    [Display(Name = "Notes")]
    public string? Notes { get; init; }

    [Required, Range(UserConsts.LagRefreshXWeeksMin, UserConsts.LagRefreshXWeeksMax)]
    [Display(Name = "Lag Refresh by X Weeks", Description = "Add a delay before this variation is recycled from your workouts.")]
    public int LagRefreshXWeeks { get; init; }

    [Required, Range(UserConsts.PadRefreshXWeeksMin, UserConsts.PadRefreshXWeeksMax)]
    [Display(Name = "Pad Refresh by X Weeks", Description = "Add a delay before this variation is recirculated back into your workouts.")]
    public int PadRefreshXWeeks { get; init; }
}
