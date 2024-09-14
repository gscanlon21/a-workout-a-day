using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// https://www.ncbi.nlm.nih.gov/books/NBK557555/
/// </summary>
[Flags]
public enum ThoracicSkills
{
    None = 0,

    /// <summary>
    /// Bending the head forward towards the chest.
    /// </summary>
    [Display(Name = "Thoracic Flexion")]
    ThoracicFlexion = 1 << 0, // 1

    /// <summary>
    /// Bending the head backward with the face towards the sky.
    /// </summary>
    [Display(Name = "Thoracic Extension")]
    ThoracicExtension = 1 << 1, // 2

    /// <summary>
    /// Turning the head to the left or the right.
    /// </summary>
    [Display(Name = "Thoracic Rotation")]
    ThoracicRotation = 1 << 2, // 4

    /// <summary>
    /// Tipping the head to the side or touching an ear to the ipsilateral shoulder.
    /// </summary>
    [Display(Name = "Thoracic Side-bending")]
    ThoracicSideBending = 1 << 3, // 8

    /// <summary>
    /// Vergence is the ability to move our eyes together to focus on a certain point.
    /// </summary>
    [Display(Name = "Thoracic Mobility")]
    ThoracicMobility = ThoracicFlexion | ThoracicExtension | ThoracicRotation | ThoracicSideBending, // 15

    /// <summary>
    /// In order to dissociate, or separate, our thoracic spine (upper back) motion from our 
    /// ... lumbar spine (lower back) motion we must have both adequate flexibility and muscular control. 
    /// Dissociation between these sections of our spine is important during reciprocal movements 
    /// ... such as walking and running where they will be rotating in opposite directions.
    /// </summary>
    [Display(Name = "Thoracic Dissociation")]
    ThoracicDissociation = 1 << 4, // 16


    All = ThoracicFlexion | ThoracicExtension | ThoracicRotation | ThoracicSideBending | ThoracicDissociation,
}
