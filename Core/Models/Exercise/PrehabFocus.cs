using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

[Flags]
public enum PrehabFocus : long
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None,

    /// <summary>
    /// Stomach muscles
    /// </summary>
    [Display(Name = "Abdominals", ShortName = "Abs")]
    Abdominals = MusculoskeletalSystem.Abdominals, // 1

    /// <summary>
    /// Front of upper arm muscles
    /// </summary>
    [Display(Name = "Biceps")]
    Biceps = MusculoskeletalSystem.Biceps, // 2

    /// <summary>
    /// Almost-shoulder muscles
    /// </summary>
    //[Display(GroupName = "Deltoids", Name = "Deltoids")]
    //Deltoids = MusculoskeletalSystem.LatDelt | MusculoskeletalSystem.RearDelt | MusculoskeletalSystem.FrontDelt, // 4

    /// <summary>
    /// Chest muscles
    /// </summary>
    [Display(Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = MusculoskeletalSystem.Pectorals, // 8

    /// <summary>
    /// Side muscles
    /// </summary>
    [Display(Name = "Obliques")]
    Obliques = MusculoskeletalSystem.Obliques, // 16

    /// <summary>
    /// Upper back muscles
    /// </summary>
    [Display(Name = "Trapezius", ShortName = "Traps")]
    Trapezius = MusculoskeletalSystem.Trapezius, // 32

    /// <summary>
    /// Back muscles
    /// </summary>
    [Display(Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = MusculoskeletalSystem.LatissimusDorsi, // 64

    /// <summary>
    /// Spinal muscles
    /// </summary>
    [Display(Name = "Spinal Erectors")]
    ErectorSpinae = MusculoskeletalSystem.ErectorSpinae, // 128

    /// <summary>
    /// Hip Abductors (Lift your leg out to the side, or from a squatting position, knees falls out to the side) 
    /// - gluteus medius and minimus.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – gluteus maximus.
    /// </summary>
    //[Display(GroupName = "Glutes", Name = "Glutes")]
    //Glutes = MusculoskeletalSystem.GluteMax | MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin, // 256

    /// <summary>
    /// Back of upper leg muscles.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – hamstrings (focus on biceps femoris).
    /// </summary>
    [Display(Name = "Hamstrings")]
    Hamstrings = MusculoskeletalSystem.Hamstrings, // 512

    /// <summary>
    /// Lower leg muscles
    /// </summary>
    [Display(Name = "Calves")]
    Calves = MusculoskeletalSystem.Calves, // 1024

    /// <summary>
    /// Front of upper leg muscles
    /// </summary>
    [Display(Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = MusculoskeletalSystem.Quadriceps, // 2048

    /// <summary>
    /// Back of upper arm muscles
    /// </summary>
    [Display(Name = "Triceps")]
    Triceps = MusculoskeletalSystem.Triceps, // 4096

    /// <summary>
    /// Hip flexors (Lift your thigh upward in front of your body) 
    /// - rectus femoris, iliopsoas, sartorius, and tensor fasciae latae.
    /// 
    /// Stretched by extending your leg behind you. Sitting shortens the hip flexors.
    /// </summary>
    [Display(Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = MusculoskeletalSystem.HipFlexors, // 8192

    /// <summary>
    /// The lower-part of the arm between the hand and the elbow.
    /// </summary>
    [Display(Name = "Forearms")]
    Forearms = MusculoskeletalSystem.Forearms, // 16384

    /// <summary>
    /// The inner thigh groin muscles. Helps move the hip laterally out to the side and across the body.
    /// 
    /// From a position of hip abduction, lower your thigh to the anatomical position.
    /// </summary>
    [Display(Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = MusculoskeletalSystem.HipAdductors, // 32768

    /// <summary>
    /// Shoulder muscles
    /// </summary>
    [Display(Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = MusculoskeletalSystem.RotatorCuffs, // 65536

    /// <summary>
    /// Deep upper-middle back muscles. Similar movements as the middle Traps.
    /// 
    /// The shoulder blades.
    /// </summary>
    [Display(Name = "Rhomboids")]
    Rhomboids = MusculoskeletalSystem.Rhomboids, // 131072

    /// <summary>
    /// Sides of the upper chest
    /// </summary>
    [Display(Name = "Serratus Anterior")]
    SerratusAnterior = MusculoskeletalSystem.SerratusAnterior, // 262144

    /// <summary>
    /// Front of the leg. Lifts the ankle up.
    /// </summary>
    [Display(Name = "Tibialis Anterior")]
    TibialisAnterior = MusculoskeletalSystem.TibialisAnterior, // 524288

    // Parts
    [Display(GroupName = "Glutes", Name = "Glute Max")]
    GluteMax = MusculoskeletalSystem.GluteMax, // 2147483648
    //[Display(GroupName = "Glutes", Name = "Glute Med")]
    //GluteMed = MusculoskeletalSystem.GluteMed, // 4294967296
    //[Display(GroupName = "Glutes", Name = "Glute Min")]
    //GluteMin = MusculoskeletalSystem.GluteMin, // 8589934592
    [Display(GroupName = "Glutes", Name = "Glute Med/Min")]
    GluteMedMin = MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin, // 4294967296 + 8589934592
    [Display(GroupName = "Deltoids", Name = "Front Deltoid")]
    FrontDelt = MusculoskeletalSystem.FrontDelt, // 34359738368
    [Display(GroupName = "Deltoids", Name = "Lateral Deltoid")]
    LatDelt = MusculoskeletalSystem.LatDelt, // 68719476736
    [Display(GroupName = "Deltoids", Name = "Rear Deltoid")]
    RearDelt = MusculoskeletalSystem.RearDelt, // 137438953472

    // Joints
    [Display(Name = "Fingers")]
    Fingers = MusculoskeletalSystem.FingerJoints, // 1048576
    [Display(Name = "Wrists")]
    Wrists = MusculoskeletalSystem.WristJoints, // 2097152
    [Display(Name = "Elbows")]
    Elbows = MusculoskeletalSystem.ElbowJoints, // 4194304
    [Display(Name = "Shoulders")]
    Shoulders = MusculoskeletalSystem.ShoulderJoints, // 8388608
    [Display(Name = "Hips")]
    Hips = MusculoskeletalSystem.HipJoints, // 16777216
    [Display(Name = "Knees")]
    Knees = MusculoskeletalSystem.KneeJoints, // 33554432
    [Display(Name = "Ankles")]
    Ankles = MusculoskeletalSystem.AnkleJoints, // 67108864
    [Display(Name = "Toes")]
    Toes = MusculoskeletalSystem.ToeJoints, // 134217728

    // Other
    [Display(Name = "Pelvic Floor")]
    PelvicFloor = MusculoskeletalSystem.PelvicFloor, // 268435456
    [Display(Name = "Eyes")]
    Eyes = MusculoskeletalSystem.Eyes, // 536870912
    [Display(Name = "Neck")]
    Neck = MusculoskeletalSystem.Neck, // 1073741824
    [Display(Name = "Nose")]
    Nose = MusculoskeletalSystem.Nose, // 274877906944
    [Display(Name = "Vocal Cords")]
    VocalCords = MusculoskeletalSystem.VocalCords, // 549755813888
    [Display(Name = "Diaphragm")]
    Diaphragm = MusculoskeletalSystem.Diaphragm, // 1099511627776

    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | GluteMax | GluteMedMin | FrontDelt | LatDelt | RearDelt
        | Fingers | Wrists | Elbows | Shoulders | Hips | Knees | Ankles | Toes
        | PelvicFloor | Eyes | Neck | Nose | VocalCords | Diaphragm
}
