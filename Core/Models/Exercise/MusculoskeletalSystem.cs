using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

[Flags]
public enum MusculoskeletalSystem : long
{
    None = 0,

    // Major muscle groups
    [Display(GroupName = "Core", Name = "Abdominals", ShortName = "Abs")]
    Abdominals = 1 << 0, // 1
    [Display(GroupName = "Arms", Name = "Biceps")]
    Biceps = 1 << 1, // 2
    [Display(GroupName = "Shoulders", Name = "Deltoids")]
    Deltoids = 1 << 2, // 4
    [Display(GroupName = "Chest", Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = 1 << 3, // 8
    [Display(GroupName = "Core", Name = "Obliques")]
    Obliques = 1 << 4, // 16
    [Display(GroupName = "Back", Name = "Trapezius", ShortName = "Traps")]
    Trapezius = 1 << 5, // 32
    [Display(GroupName = "Back", Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = 1 << 6, // 64
    [Display(GroupName = "Core", Name = "Spinal Erector")]
    ErectorSpinae = 1 << 7, // 128
    [Display(GroupName = "Legs", Name = "Glutes")]
    Glutes = 1 << 8, // 256
    [Display(GroupName = "Legs", Name = "Hamstrings")]
    Hamstrings = 1 << 9, // 512
    [Display(GroupName = "Legs", Name = "Calves")]
    Calves = 1 << 10, // 1024
    [Display(GroupName = "Legs", Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = 1 << 11, // 2048
    [Display(GroupName = "Arms", Name = "Triceps")]
    Triceps = 1 << 12, // 4096
    [Display(GroupName = "Legs", Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = 1 << 13, // 8192
    [Display(GroupName = "Arms", Name = "Forearms")]
    Forearms = 1 << 14, // 16384
    [Display(GroupName = "Legs", Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = 1 << 15, // 32768
    [Display(GroupName = "Shoulders", Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = 1 << 16, // 65536
    [Display(GroupName = "Back", Name = "Rhomboids")]
    Rhomboids = 1 << 17, // 131072
    [Display(GroupName = "Chest", Name = "Serratus Anterior")]
    SerratusAnterior = 1 << 18, // 262144
    [Display(GroupName = "Legs", Name = "Tibialis Anterior")]
    TibialisAnterior = 1 << 19, // 524288

    // Major joints
    [Display(Name = "Finger Joints")]
    FingerJoints = 1 << 20, // 1048576
    [Display(Name = "Wrist Joints")]
    WristJoints = 1 << 21, // 2097152
    [Display(Name = "Elbow Joints")]
    ElbowJoints = 1 << 22, // 4194304
    [Display(Name = "Shoulder Joints")]
    ShoulderJoints = 1 << 23, // 8388608
    [Display(Name = "Hip Joints")]
    HipJoints = 1 << 24, // 16777216
    [Display(Name = "Knee Joints")]
    KneeJoints = 1 << 25, // 33554432
    [Display(Name = "Ankle Joints")]
    AnkleJoints = 1 << 26, // 67108864
    [Display(Name = "Toe Joints")]
    ToeJoints = 1 << 27, // 134217728

    // Other
    [Display(Name = "Pelvic Floor")]
    PelvicFloor = 1 << 28, // 268435456
    [Display(Name = "Eyes")]
    Eyes = 1 << 29, // 536870912
    [Display(Name = "Neck")]
    Neck = 1 << 30, // 1073741824

    // Major Muscle Group Parts
    [Display(GroupName = "Legs", Name = "Glute Max")]
    GluteMax = Glutes | 1L << 31, // 256 | 2147483648
    [Display(GroupName = "Legs", Name = "Glute Med")]
    GluteMed = Glutes | 1L << 32, // 256 | 4294967296
    [Display(GroupName = "Legs", Name = "Glute Min")]
    GluteMin = Glutes | 1L << 33, // 256 | 8589934592

    [Display(GroupName = "Shoulders", Name = "Front Deltoid")]
    FrontDelt = Deltoids | 1L << 34, // 4 | 17179869184
    [Display(GroupName = "Shoulders", Name = "Lateral Deltoid")]
    LatDelt = Deltoids | 1L << 35, // 4 | 34359738368
    [Display(GroupName = "Shoulders", Name = "Rear Deltoid")]
    RearDelt = Deltoids | 1L << 36, // 4 | 68719476736

    // No skeletons

    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | Glutes | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | Deltoids | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | FingerJoints | WristJoints | ElbowJoints | ShoulderJoints | HipJoints | KneeJoints | AnkleJoints | ToeJoints
        | PelvicFloor | Eyes | Neck
        | GluteMax | GluteMed | GluteMin | FrontDelt | LatDelt | RearDelt
}
