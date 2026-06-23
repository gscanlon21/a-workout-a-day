using System.ComponentModel.DataAnnotations;

namespace Core.Models.Newsletter;

[Flags]
public enum ExtendedWarmup
{
    [Display(Name = "None", Description = "Only warmup the muscle groups that are targeted in your workout.")]
    None = 0,

    [Display(Name = "All Muscle Groups", Description = "Warmup all muscle groups regardless of whether they are in your workout.")]
    AllMuscleGroups = 1 << 0, // 1

    [Display(Name = "Mobilize Joints", Description = "Add joint mobilization exercises working the same joints as your workout.")]
    JointMobilization = 1 << 1, // 2
}
