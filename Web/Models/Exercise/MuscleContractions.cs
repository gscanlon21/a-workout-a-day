using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

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
    [Display(Name = "Concentric", GroupName = "Dynamic")]
    Concentric = 1 << 1, // 2

    /// <summary>
    /// The muscle contracts and lengthens. Pushing motion.
    /// </summary>
    [Display(Name = "Eccentric", GroupName = "Dynamic")]
    Eccentric = 1 << 2, // 4

    Dynamic = Concentric | Eccentric, // 6

    All = Static | Dynamic // 7
}
