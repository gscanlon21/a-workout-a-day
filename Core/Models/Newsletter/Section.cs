using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Newsletter;

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

    [Display(Name = "Warmup Raise")]
    WarmupRaise = 1 << 3, // 8
    [Display(Name = "Warmup Activation/Mobilization")]
    WarmupActivationMobilization = 1 << 4, // 16
    [Display(Name = "Warmup Potentiation")]
    WarmupPotentiation = 1 << 5, // 32
    [Display(Name = "Warmup")]
    Warmup = WarmupRaise | WarmupPotentiation | WarmupActivationMobilization, // 56

    [Display(Name = "Sports Plyometric")]
    SportsPlyometric = 1 << 6, // 64
    [Display(Name = "Sports Strengthening")]
    SportsStrengthening = 1 << 7, // 128
    [Display(Name = "Sports")]
    Sports = SportsPlyometric | SportsStrengthening, // 192

    Debug = 1 << 30, // 1073741824
    [Display(Name = "Functional")]
    Functional = 1 << 8, // 256
    [Display(Name = "Accessory")]
    Accessory = 1 << 9, // 512
    [Display(Name = "Core")]
    Core = 1 << 10, // 1024
    [Display(Name = "Main")]
    Main = Functional | Accessory | Core | Debug, // 1792 + 2147483648

    [Display(Name = "Prehab Strengthening")]
    PrehabStrengthening = 1 << 11, // 2048
    [Display(Name = "Prehab Stretching")]
    PrehabStretching = 1 << 12, // 4096
    [Display(Name = "Prehab")]
    Prehab = PrehabStrengthening | PrehabStretching, // 6144

    [Display(Name = "Cooldown Stretching")]
    CooldownStretching = 1 << 13, // 8192
    [Display(Name = "Mindfulness")]
    Mindfulness = 1 << 14, // 16384
    [Display(Name = "Cooldown")]
    Cooldown = CooldownStretching | Mindfulness, // 24576

    All = Rehab | Warmup | Sports | Main | Prehab | Cooldown, // 32767 + 32768
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