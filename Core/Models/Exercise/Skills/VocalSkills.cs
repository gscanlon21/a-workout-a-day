using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// Speech therapy skills.
/// </summary>
[Flags]
public enum VocalSkills
{
    None = 0,

    /// <summary>
    /// The ability to control airflow.
    /// </summary>
    [Display(Name = "Breath Control")]
    BreathControl = 1 << 0, // 1

    /// <summary>
    /// The ability to match and maintain pitch.
    /// </summary>
    [Display(Name = "Pitch Control")]
    PitchControl = 1 << 1, // 2

    /// <summary>
    /// The controlled oscillation of pitch.
    /// </summary>
    [Display(Name = "Vibrato")]
    Vibrato = 1 << 2, // 4

    /// <summary>
    /// Clarity and precision in forming words.
    /// </summary>
    [Display(Name = "Diction")]
    Diction = 1 << 3, // 8

    /// <summary>
    /// The ability to separate sounds and syllables clearly.
    /// </summary>
    [Display(Name = "Articulation")]
    Articulation = 1 << 4, // 16

    /// <summary>
    /// The development of a wider vocal range across pitches.
    /// </summary>
    [Display(Name = "Range Expansion")]
    RangeExpansion = 1 << 5, // 32

    /// <summary>
    /// Clear and distinct pronunciation of syllables.
    /// </summary>
    [Display(Name = "Enunciation")]
    Enunciation = 1 << 6, // 64

    /// <summary>
    /// Correct pronuncitation of words.
    /// </summary>
    [Display(Name = "Pronunciation")]
    Pronunciation = 1 << 7, // 128

    /// <summary>
    /// The ability to vary tone, pitch, and dynamics.
    /// </summary>
    [Display(Name = "Modulation")]
    Modulation = 1 << 8, // 256

    /// <summary>
    /// The richness and quality of sound produced by the voice.
    /// </summary>
    [Display(Name = "Resonance")]
    Resonance = 1 << 9, // 512


    All = BreathControl | PitchControl | Vibrato | Diction | Articulation | RangeExpansion | Enunciation | Pronunciation | Modulation | Resonance
}
