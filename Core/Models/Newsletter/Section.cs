using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Newsletter;

/// <summary>
/// To edit these (you must run these in descending order):
/// await _context.Variations.Where(a => a.Section.HasFlag(Section.Mindfulness)).ExecuteUpdateAsync(a => a.SetProperty(x => x.Section, x => x.Section - (int)Section.Mindfulness + (int)SectionNew.Mindfulness));
/// </summary>
[Flags]
public enum Section
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Rehab Mechanics")]
    RehabMechanics = 1 << 0, // 1

    [Display(Name = "Rehab Velocity")]
    RehabVelocity = 1 << 1, // 2

    [Display(Name = "Rehab Strengthening")]
    RehabStrengthening = 1 << 2, // 4

    [Display(Name = "Rehab")]
    Rehab = RehabMechanics | RehabVelocity | RehabStrengthening, // 7

    /// <summary>Raises the heartrate.</summary>
    [Display(Name = "Warmup Raise")]
    WarmupRaise = 1 << 3, // 8

    /// <summary>Activates muscle groups.</summary>
    [Display(Name = "Warmup Activation")]
    WarmupActivation = 1 << 4, // 16

    /// <summary>Activates joint movements.</summary>
    [Display(Name = "Warmup Mobilization")]
    WarmupMobilization = 1 << 5, // 32

    [Display(Name = "Warmup Activation/Mobilization")]
    WarmupActivationMobilization = WarmupActivation | WarmupMobilization, // 48

    /// <summary>Primes the muscle groups</summary>
    [Display(Name = "Warmup Potentiation")]
    WarmupPotentiation = 1 << 6, // 64

    [Display(Name = "Warmup")]
    Warmup = WarmupRaise | WarmupActivation | WarmupMobilization | WarmupPotentiation, // 120


    [Display(Name = "Sports Power")]
    SportsPower = 1 << 7, // 128

    [Display(Name = "Sports Control")]
    SportsControl = 1 << 8, // 256

    [Display(Name = "Sports Strengthening")]
    SportsStrengthening = 1 << 9, // 512

    [Display(Name = "Sports")]
    Sports = SportsPower | SportsControl | SportsStrengthening, // 896


    Debug = 1 << 30, // 1073741824

    [Display(Name = "Functional")]
    Functional = 1 << 10, // 1024

    [Display(Name = "Accessory")]
    Accessory = 1 << 11, // 2048

    [Display(Name = "Core")]
    Core = 1 << 12, // 4096

    [Display(Name = "Main")]
    Main = Functional | Accessory | Core | Debug, // 7168 + 1073741824


    [Display(Name = "Prehab Strengthening")]
    PrehabStrengthening = 1 << 13, // 8192

    [Display(Name = "Prehab Stabilization")]
    PrehabStabilization = 1 << 14, // 16384

    [Display(Name = "Prehab Stretching")]
    PrehabStretching = 1 << 15, // 32768

    [Display(Name = "Prehab")]
    Prehab = PrehabStrengthening | PrehabStabilization | PrehabStretching, // 57344


    [Display(Name = "Cooldown Stabilization")]
    CooldownStabilization = 1 << 16, // 65536

    [Display(Name = "Cooldown Stretching")]
    CooldownStretching = 1 << 17, // 131072

    [Display(Name = "Mindfulness")]
    Mindfulness = 1 << 18, // 262144

    [Display(Name = "Cooldown")]
    Cooldown = CooldownStabilization | CooldownStretching | Mindfulness, // 458752


    All = Rehab | Warmup | Sports | Main | Prehab | Cooldown, // 524287
}

public static class SectionExtensions
{
    public static ExerciseTheme AsTheme(this Section section) => section switch
    {
        not Section.None when Section.Warmup.HasFlag(section) => ExerciseTheme.Warmup,
        not Section.None when Section.Cooldown.HasFlag(section) => ExerciseTheme.Cooldown,
        not Section.None when Section.Main.HasFlag(section) => ExerciseTheme.Main,
        not Section.None when Section.Sports.HasFlag(section) => ExerciseTheme.Other,
        not Section.None when Section.Rehab.HasFlag(section) => ExerciseTheme.Extra,
        not Section.None when Section.Prehab.HasFlag(section) => ExerciseTheme.Extra,
        _ => ExerciseTheme.None,
    };
}
