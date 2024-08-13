using Core.Consts;
using Core.Models.User;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.User;

namespace Web.Components.User;

/// <summary>
/// Renders an the edit form for the user.
/// </summary>
public class EditViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Edit";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User? user = null)
    {
        user ??= await userRepo.GetUser(UserConsts.DemoUser, UserConsts.DemoToken, includeExerciseVariations: true, includeMuscles: true, includeFrequencies: true, allowDemoUser: true);
        if (user == null)
        {
            return Content("");
        }

        var token = await userRepo.AddUserToken(user, durationDays: 1);
        return View("Edit", await PopulateUserEditViewModel(new UserEditViewModel(user, token)));
    }

    private async Task<UserEditViewModel> PopulateUserEditViewModel(UserEditViewModel viewModel)
    {
        viewModel.UserFrequencies = (viewModel.UserFrequencies?.NullIfEmpty() ?? new WorkoutSplit(Frequency.FullBody2Day).OrderBy(f => f.Id).Select(f => new UserEditViewModel.UserEditFrequencyViewModel(f))).ToList();
        while (viewModel.UserFrequencies.Count < UserConsts.MaxUserFrequencies)
        {
            viewModel.UserFrequencies.Add(new UserEditViewModel.UserEditFrequencyViewModel() { Day = viewModel.UserFrequencies.Count + 1 });
        }

        foreach (var muscleGroup in UserMuscleMobility.MuscleTargets.Keys
            .OrderBy(mg => mg.GetSingleDisplayName(DisplayType.GroupName))
            .ThenBy(mg => mg.GetSingleDisplayName()))
        {
            var userMuscleMobility = viewModel.User.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == muscleGroup);
            viewModel.UserMuscleMobilities.Add(userMuscleMobility != null ? new UserEditViewModel.UserEditMuscleMobilityViewModel(userMuscleMobility) : new UserEditViewModel.UserEditMuscleMobilityViewModel()
            {
                UserId = viewModel.User.Id,
                MuscleGroup = muscleGroup,
                Count = UserMuscleMobility.MuscleTargets.TryGetValue(muscleGroup, out int countTmp) ? countTmp : 0
            });
        }

        foreach (var muscleGroup in UserMuscleFlexibility.MuscleTargets.Keys
            .OrderBy(mg => mg.GetSingleDisplayName(DisplayType.GroupName))
            .ThenBy(mg => mg.GetSingleDisplayName()))
        {
            var userMuscleFlexibility = viewModel.User.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == muscleGroup);
            viewModel.UserMuscleFlexibilities.Add(userMuscleFlexibility != null ? new UserEditViewModel.UserEditMuscleFlexibilityViewModel(userMuscleFlexibility) : new UserEditViewModel.UserEditMuscleFlexibilityViewModel()
            {
                UserId = viewModel.User.Id,
                MuscleGroup = muscleGroup,
                Count = UserMuscleFlexibility.MuscleTargets.TryGetValue(muscleGroup, out int countTmp) ? countTmp : 0
            });
        }

        return viewModel;
    }
}
