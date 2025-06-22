﻿using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// The components of an exercise.
/// </summary>
[Flags]
public enum ExerciseFocus
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Muscle strength.
    /// </summary>
    [Display(Name = "Strength")]
    Strength = 1 << 0, // 1

    /// <summary>
    /// Muscle speed.
    /// Generally involves the Stretch-Shortening Cycle (SSC)
    /// ... commonly seen in plyometric musle movements.
    /// </summary>
    [Display(Name = "Speed")]
    Speed = 1 << 1, // 2

    /// <summary>
    /// Muscle power.
    /// </summary>
    [Display(Name = "Power")]
    Power = Strength | Speed, // 3

    /// <summary>
    /// Muscle output sustained for an extended duration.
    /// </summary>
    [Display(Name = "Endurance")]
    Endurance = 1 << 2, // 4

    /// <summary>
    /// Muscle output sustained near 100% for an extended duration.
    /// </summary>
    [Display(Name = "Stamina")]
    Stamina = Strength | Endurance, // 5

    /// <summary>
    /// Muscle output sustained near 100% for an extended duration.
    /// </summary>
    [Display(Name = "Speed Endurance")]
    SpeedEndurance = Speed | Endurance, // 6

    /// <summary>
    /// Muscle range of motion.
    /// </summary>
    [Display(Name = "Flexibility")]
    Flexibility = 1 << 3, // 8

    /// <summary>
    /// Muscle control. Balance work.
    /// </summary>
    [Display(Name = "Stability")]
    Stability = 1 << 4, // 16

    /// <summary>
    /// The ability to control directional changes.
    /// </summary>
    /// <seealso cref="https://www.scienceforsport.com/agility/"/>
    [Display(Name = "Agility")]
    Agility = Speed | Stability, // 18

    /// <summary>
    /// The ability for a joint to move through its entire range of motion with control.
    /// </summary>
    [Display(Name = "Mobility")]
    Mobility = Flexibility | Stability | Strength, // 25

    /// <summary>
    /// Develops the mind-body connection. Awareness. Isolation work.
    /// </summary>
    [Display(Name = "Activation")]
    Activation = 1 << 5, // 32

    All = Strength | Power | Endurance | Flexibility | Stability | Mobility | Speed | Agility | Stamina | Activation
}
