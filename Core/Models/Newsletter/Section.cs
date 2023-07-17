namespace Core.Models.Newsletter;

[Flags]
public enum Section
{
    None = 0,

    RehabWarmup = 1 << 0, // 1
    RehabMain = 1 << 1, // 2
    RehabCooldown = 1 << 2, // 4
    Rehab = RehabWarmup | RehabMain | RehabCooldown,

    WarmupRaise = 1 << 3, // 8
    WarmupActivationMobilization = 1 << 4, // 16
    WarmupPotentiationPerformance = 1 << 5, // 32
    Warmup = WarmupRaise | WarmupPotentiationPerformance | WarmupActivationMobilization,

    SportsPlyometric = 1 << 6, // 64
    SportsStrengthening = 1 << 7, // 128
    Sports = SportsPlyometric | SportsStrengthening,

    Functional = 1 << 8, // 256
    Accessory = 1 << 9, // 512
    Core = 1 << 10, // 1024
    Main = Functional | Accessory | Core,

    PrehabStrengthening = 1 << 11, // 2048
    PrehabCooldown = 1 << 12, // 4096
    Prehab = PrehabStrengthening | PrehabCooldown,

    CooldownStretching = 1 << 13, // 8192
    Mindfulness = 1 << 14, // 16384
    Cooldown = CooldownStretching | Mindfulness,
}