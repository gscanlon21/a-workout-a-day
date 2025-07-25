﻿using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// https://www.ncbi.nlm.nih.gov/books/NBK557555/
/// </summary>
[Flags]
public enum LumbarSkills
{
    None = 0,

    /// <summary>
    /// Bending the head forward towards the chest.
    /// </summary>
    [Display(Name = "Lumbar Flexion")]
    LumbarFlexion = 1 << 0, // 1

    /// <summary>
    /// Bending the head backward with the face towards the sky.
    /// </summary>
    [Display(Name = "Lumbar Extension")]
    LumbarExtension = 1 << 1, // 2

    /// <summary>
    /// Turning the head to the left or the right.
    /// </summary>
    [Display(Name = "Lumbar Rotation")]
    LumbarRotation = 1 << 2, // 4

    /// <summary>
    /// Tipping the head to the side or touching an ear to the ipsilateral shoulder.
    /// </summary>
    [Display(Name = "Lumbar Side-bending")]
    LumbarSideBending = 1 << 3, // 8

    /// <summary>
    /// Vergence is the ability to move our eyes together to focus on a certain point.
    /// </summary>
    [Display(Name = "Lumbar Mobility")]
    LumbarMobility = LumbarFlexion | LumbarExtension | LumbarRotation | LumbarSideBending, // 15

    /// <summary>
    /// In order to dissociate, or separate, our Lumbar spine (upper back) motion from our 
    /// ... lumbar spine (lower back) motion we must have both adequate flexibility and muscular control. 
    /// Dissociation between these sections of our spine is important during reciprocal movements 
    /// ... such as walking and running where they will be rotating in opposite directions.
    /// </summary>
    [Display(Name = "Lumbar Dissociation")]
    LumbarDissociation = 1 << 4, // 16


    All = LumbarFlexion | LumbarExtension | LumbarRotation | LumbarSideBending | LumbarDissociation,
}
