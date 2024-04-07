using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

[Flags]
public enum MusculoskeletalSystem : long
{
    None = 0,

    // Muscle Groups
    [Display(Name = "Abdominals", ShortName = "Abs")]
    Abdominals = 1 << 0, // 1
    [Display(Name = "Biceps")]
    Biceps = 1 << 1, // 2
    //[Display(GroupName = "Deltoids", Name = "Deltoids")]
    //Deltoids = 1 << 2, // 4
    [Display(Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = 1 << 3, // 8
    [Display(Name = "Obliques")]
    Obliques = 1 << 4, // 16
    [Display(Name = "Trapezius", ShortName = "Traps")]
    Trapezius = 1 << 5, // 32
    [Display(Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = 1 << 6, // 64
    [Display(Name = "Spinal Erectors")]
    ErectorSpinae = 1 << 7, // 128
    //[Display(GroupName = "Glutes", Name = "Glutes")]
    //Glutes = 1 << 8, // 256
    [Display(Name = "Hamstrings")]
    Hamstrings = 1 << 9, // 512
    [Display(Name = "Calves")]
    Calves = 1 << 10, // 1024
    [Display(Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = 1 << 11, // 2048
    [Display(Name = "Triceps")]
    Triceps = 1 << 12, // 4096
    [Display(Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = 1 << 13, // 8192
    [Display(Name = "Forearms")]
    Forearms = 1 << 14, // 16384
    [Display(Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = 1 << 15, // 32768
    [Display(Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = 1 << 16, // 65536
    [Display(Name = "Rhomboids")]
    Rhomboids = 1 << 17, // 131072
    [Display(Name = "Serratus Anterior")]
    SerratusAnterior = 1 << 18, // 262144
    [Display(Name = "Tibialis Anterior")]
    TibialisAnterior = 1 << 19, // 524288

    // Joints
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
    [Display(Name = "Nose")]
    Nose = 1L << 38, // 274877906944
    [Display(Name = "Vocal Cords")]
    VocalCords = 1L << 39, // 549755813888
    [Display(Name = "Diaphragm")]
    Diaphragm = 1L << 40, // 1099511627776
    [Display(Name = "Throat")]
    Throat = 1L << 41, // 2199023255552

    // Muscles
    [Display(GroupName = "Glutes", Name = "Glute Max")]
    GluteMax = 1L << 31, // 2147483648
    [Display(GroupName = "Glutes", Name = "Glute Med")]
    GluteMed = 1L << 32, // 4294967296
    [Display(GroupName = "Glutes", Name = "Glute Min")]
    GluteMin = 1L << 33, // 8589934592
    //[Display(GroupName = "Glutes", Name = "Glute Med/Min")]
    //GluteMedMin = 1L << 34, // 17179869184

    [Display(GroupName = "Deltoids", Name = "Front Deltoid")]
    FrontDelt = 1L << 35, // 4 + 34359738368
    [Display(GroupName = "Deltoids", Name = "Lateral Deltoid")]
    LatDelt = 1L << 36, // 4 + 68719476736
    [Display(GroupName = "Deltoids", Name = "Rear Deltoid")]
    RearDelt = 1L << 37, // 4 + 137438953472

    // No skeletons

    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | FingerJoints | WristJoints | ElbowJoints | ShoulderJoints | HipJoints | KneeJoints | AnkleJoints | ToeJoints
        | PelvicFloor | Eyes | Neck | Nose | VocalCords | Diaphragm | Throat
        | GluteMax | GluteMed | GluteMin | FrontDelt | LatDelt | RearDelt
}
