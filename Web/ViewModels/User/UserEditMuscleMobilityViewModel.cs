﻿using Core.Consts;
using Core.Models.Exercise;
using Data.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

public class UserEditMuscleMobilityViewModel
{
    public UserEditMuscleMobilityViewModel() { }

    public UserEditMuscleMobilityViewModel(UserMuscleMobility userMuscleMobility)
    {
        UserId = userMuscleMobility.UserId;
        MuscleGroup = userMuscleMobility.MuscleGroup;
        Count = userMuscleMobility.Count;
    }

    public MuscleGroups MuscleGroup { get; init; }

    public int UserId { get; init; }

    [Range(UserConsts.UserMuscleMobilityMin, UserConsts.UserMuscleMobilityMax)]
    public int Count { get; set; }
}
