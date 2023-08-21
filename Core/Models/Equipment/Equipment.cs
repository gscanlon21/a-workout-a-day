using System.ComponentModel.DataAnnotations;

namespace Core.Models.Equipment;


/// <summary>
/// Equipment used in an exercise.
/// </summary>
public enum Equipment
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Dumbbells")]
    Dumbbells = 1 << 0, // 1

    [Display(Name = "Kettlebells")]
    Kettlebells = 1 << 1, // 2

    [Display(Name = "Barbell")]
    Barbell = 1 << 2, // 4

    [Display(Name = "Plates")]
    Plates = 1 << 3, // 8

    [Display(Name = "Medicine Ball")]
    MedicineBall = 1 << 4, // 16

    [Display(Name = "Slam Ball")]
    SlamBall = 1 << 5, // 32

    [Display(Name = "Resistance Bands")]
    ResistanceBands = 1 << 6, // 64

    [Display(Name = "Mini Loop Bands")]
    MiniLoopBands = 1 << 7, // 128

    [Display(Name = "Pullup Bar")]
    PullupBar = 1 << 8, // 256

    [Display(Name = "Gymnastic Rings")]
    GymnasticRings = 1 << 9, // 512

    [Display(Name = "TRX System")]
    TRXSystem = 1 << 10, // 1024

    [Display(Name = "Low Box")]
    LowBox = 1 << 11, // 2048

    [Display(Name = "High Box")]
    HighBox = 1 << 12, // 4096

    [Display(Name = "Stability Ball")]
    StabilityBall = 1 << 13, // 8192

    [Display(Name = "Flat Bench")]
    FlatBench = 1 << 14, // 16384

    [Display(Name = "Incline Bench")]
    InclineBench = 1 << 15, // 32768

    [Display(Name = "Jump Rope")]
    JumpRope = 1 << 16, // 65536

    [Display(Name = "Hula Hoop")]
    HulaHoop = 1 << 17, // 131072

    All = Dumbbells | Kettlebells | Barbell | Plates | MedicineBall | SlamBall | ResistanceBands | MiniLoopBands | GymnasticRings
        | PullupBar | TRXSystem | LowBox | HighBox | StabilityBall | FlatBench | InclineBench | JumpRope | HulaHoop
}
