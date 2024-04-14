using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Isometric/Concentric/Eccentric/Plyometric.
/// </summary>
[Flags]
public enum MuscleMovement
{
    /// <summary>
    /// Same length. This type of exercise involves the contraction of a muscle without movement of the joint.
    /// </summary>
    [Display(Name = "Isometric")]
    Isometric = 1 << 0, // 1

    /// <summary>
    /// Same tension. This type of exercise is dynamic and requires concentric and eccentric movement in a joint.
    /// </summary>
    [Display(Name = "Isotonic")]
    Isotonic = 1 << 1, // 2

    /// <summary>
    /// Same speed. An isokinetic exercise involves shortening and lengthening a muscle at a constant speed.
    /// </summary>
    [Display(Name = "Isokinetic")]
    Isokinetic = 1 << 2, // 4

    All = Isometric | Isotonic | Isokinetic
}
