using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Isometric/Concentric/Eccentric.
/// </summary>
[Flags]
public enum MuscleContractions
{
    /// <summary>
    /// Isometric exercises involve constant muscle contraction without changing the actual length of your muscles.
    /// </summary>
    [Display(Name = "Static")]
    Static = 1 << 0, // 1

    /// <summary>
    /// The muscle contracts and shortens. Pulling motion. 
    /// </summary>
    [Display(GroupName = "Dynamic", Name = "Concentric")]
    Concentric = 1 << 1, // 2

    /// <summary>
    /// The muscle contracts and lengthens. Pushing motion.
    /// </summary>
    [Display(GroupName = "Dynamic", Name = "Eccentric")]
    Eccentric = 1 << 2, // 4

    [Display(Name = "Dynamic")]
    Dynamic = Concentric | Eccentric, // 6

    /// <summary>
    /// This is not user-facing. 
    /// It should not have a Display attribute. 
    /// </summary>
    All = Static | Dynamic // 7
}
