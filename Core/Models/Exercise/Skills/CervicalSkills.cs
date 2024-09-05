using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// https://www.ncbi.nlm.nih.gov/books/NBK557555/
/// </summary>
[Flags]
public enum CervicalSkills
{
    None = 0,

    /// <summary>
    /// Bending the head forward towards the chest.
    /// </summary>
    [Display(Name = "Cervical Flexion")]
    CervicalFlexion = 1 << 0, // 1

    /// <summary>
    /// Bending the head backward with the face towards the sky.
    /// </summary>
    [Display(Name = "Cervical Extension")]
    CervicalExtension = 1 << 1, // 2

    /// <summary>
    /// Turning the head to the left or the right.
    /// </summary>
    [Display(Name = "Cervical Rotation")]
    CervicalRotation = 1 << 2, // 4

    /// <summary>
    /// Tipping the head to the side or touching an ear to the ipsilateral shoulder.
    /// </summary>
    [Display(Name = "Cervical Side-bending")]
    CervicalSideBending = 1 << 3, // 8

    /// <summary>
    /// Vergence is the ability to move our eyes together to focus on a certain point.
    /// </summary>
    [Display(Name = "Cervical Movements")]
    CervicalMovements = CervicalFlexion | CervicalExtension | CervicalRotation | CervicalSideBending, // 15

    /// <summary>
    /// Muscle trigger-point releases.
    /// </summary>
    [Display(Name = "Cervical Tension")]
    CervicalTension = 1 << 4, // 16

    /// <summary>
    /// Trigeminal Dysphoria.
    /// </summary>
    [Display(Name = "Trigeminal Dysphoria")]
    TrigeminalDysphoria = 1 << 5, // 32

    All = CervicalFlexion | CervicalExtension | CervicalRotation | CervicalSideBending | CervicalTension | TrigeminalDysphoria,
}
