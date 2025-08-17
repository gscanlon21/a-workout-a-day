using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// 
/// </summary>
[Flags]
public enum VocalSkills
{
    None = 0,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Breath Control")]
    BreathControl = 1 << 0, // 1

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Pitch Control")]
    PitchControl = 1 << 1, // 2

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Vibrato")]
    Vibrato = 1 << 2, // 4

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Diction")]
    Diction = 1 << 3, // 8

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Articulation")]
    Articulation = 1 << 4, // 16

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Range Expansion")]
    RangeExpansion = 1 << 5, // 32

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Enunciation")]
    Enunciation = 1 << 6, // 64

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Pronunciation")]
    Pronunciation = 1 << 7, // 128


    All = BreathControl | PitchControl | Vibrato | Diction | Articulation | RangeExpansion | Enunciation | Pronunciation
}
