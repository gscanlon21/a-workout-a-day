using System.ComponentModel.DataAnnotations;

namespace Core.Models.Newsletter;

[Flags]
public enum Section
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Rehab Warmup")]
    RehabWarmup = 1 << 0, // 1
    [Display(Name = "Rehab Main")]
    RehabMain = 1 << 1, // 2
    [Display(Name = "Rehab Cooldown")]
    RehabCooldown = 1 << 2, // 4
    [Display(Name = "Rehab")]
    Rehab = RehabWarmup | RehabMain | RehabCooldown,

    [Display(Name = "Warmup Raise")]
    WarmupRaise = 1 << 3, // 8
    [Display(Name = "Warmup Activation/Mobilization")]
    WarmupActivationMobilization = 1 << 4, // 16
    [Display(Name = "Warmup Potentiation/Performance")]
    WarmupPotentiationPerformance = 1 << 5, // 32
    [Display(Name = "Warmup")]
    Warmup = WarmupRaise | WarmupPotentiationPerformance | WarmupActivationMobilization,

    [Display(Name = "Sports Plyometric")]
    SportsPlyometric = 1 << 6, // 64
    [Display(Name = "Sports Strengthening")]
    SportsStrengthening = 1 << 7, // 128
    [Display(Name = "Sports")]
    Sports = SportsPlyometric | SportsStrengthening,

    [Display(Name = "Functional")]
    Functional = 1 << 8, // 256
    [Display(Name = "Accessory")]
    Accessory = 1 << 9, // 512
    [Display(Name = "Core")]
    Core = 1 << 10, // 1024
    [Display(Name = "Main")]
    Main = Functional | Accessory | Core,

    [Display(Name = "Prehab Strengthening")]
    PrehabStrengthening = 1 << 11, // 2048
    [Display(Name = "Prehab Stretching")]
    PrehabStretching = 1 << 12, // 4096
    [Display(Name = "Prehab")]
    Prehab = PrehabStrengthening | PrehabStretching,

    [Display(Name = "Cooldown Stretching")]
    CooldownStretching = 1 << 13, // 8192
    [Display(Name = "Mindfulness")]
    Mindfulness = 1 << 14, // 16384
    [Display(Name = "Cooldown")]
    Cooldown = CooldownStretching | Mindfulness,
}