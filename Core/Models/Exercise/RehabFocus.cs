using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Major muscle groups of the body. We are working all of these muscle groups out for a full-body workout.
/// </summary>
[Flags]
public enum RehabFocus : long
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None,

    /// <summary>
    /// Stomach muscles
    /// </summary>
    [Display(GroupName = "Core", Name = "Abdominals", ShortName = "Abs")]
    Abdominals = MusculoskeletalSystem.Abdominals, // 1

    /// <summary>
    /// Front of upper arm muscles
    /// </summary>
    [Display(GroupName = "Arms", Name = "Biceps")]
    Biceps = MusculoskeletalSystem.Biceps, // 2

    /// <summary>
    /// Almost-shoulder muscles
    /// </summary>
    //[Display(GroupName = "Shoulders", Name = "Deltoids")]
    //Deltoids = MusculoskeletalSystem.FrontDelt | MusculoskeletalSystem.RearDelt | MusculoskeletalSystem.LatDelt, // 4

    /// <summary>
    /// Chest muscles
    /// </summary>
    [Display(GroupName = "Chest", Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = MusculoskeletalSystem.Pectorals, // 8

    /// <summary>
    /// Side muscles
    /// </summary>
    [Display(GroupName = "Core", Name = "Obliques")]
    Obliques = MusculoskeletalSystem.Obliques, // 16

    /// <summary>
    /// Upper back muscles
    /// </summary>
    [Display(GroupName = "Back", Name = "Trapezius", ShortName = "Traps")]
    Trapezius = MusculoskeletalSystem.Trapezius, // 32

    /// <summary>
    /// Back muscles
    /// </summary>
    [Display(GroupName = "Back", Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = MusculoskeletalSystem.LatissimusDorsi, // 64

    /// <summary>
    /// Spinal muscles
    /// </summary>
    [Display(GroupName = "Core", Name = "Spinal Erectors")]
    ErectorSpinae = MusculoskeletalSystem.ErectorSpinae, // 128

    /// <summary>
    /// Hip Abductors (Lift your leg out to the side, or from a squatting position, knees falls out to the side) 
    /// - gluteus medius and minimus.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – gluteus maximus.
    /// </summary>
    //[Display(GroupName = "Legs", Name = "Glutes")]
    //Glutes = MusculoskeletalSystem.GluteMin | MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMax, // 256

    /// <summary>
    /// Back of upper leg muscles.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – hamstrings (focus on biceps femoris).
    /// </summary>
    [Display(GroupName = "Legs", Name = "Hamstrings")]
    Hamstrings = MusculoskeletalSystem.Hamstrings, // 512

    /// <summary>
    /// Lower leg muscles
    /// </summary>
    [Display(GroupName = "Legs", Name = "Calves")]
    Calves = MusculoskeletalSystem.Calves, // 1024

    /// <summary>
    /// Front of upper leg muscles
    /// </summary>
    [Display(GroupName = "Legs", Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = MusculoskeletalSystem.Quadriceps, // 2048

    /// <summary>
    /// Back of upper arm muscles
    /// </summary>
    [Display(GroupName = "Arms", Name = "Triceps")]
    Triceps = MusculoskeletalSystem.Triceps, // 4096

    /// <summary>
    /// Hip flexors (Lift your thigh upward in front of your body) 
    /// - rectus femoris, iliopsoas, sartorius, and tensor fasciae latae.
    /// 
    /// Stretched by extending your leg behind you. Sitting shortens the hip flexors.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = MusculoskeletalSystem.HipFlexors, // 8192

    /// <summary>
    /// The lower-part of the arm between the hand and the elbow.
    /// </summary>
    [Display(GroupName = "Arms", Name = "Forearms")]
    Forearms = MusculoskeletalSystem.Forearms, // 16384

    /// <summary>
    /// The inner thigh groin muscles. Helps move the hip laterally out to the side and across the body.
    /// 
    /// From a position of hip abduction, lower your thigh to the anatomical position.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = MusculoskeletalSystem.HipAdductors, // 32768

    /// <summary>
    /// Shoulder muscles
    /// </summary>
    [Display(GroupName = "Shoulders", Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = MusculoskeletalSystem.RotatorCuffs, // 65536

    /// <summary>
    /// Deep upper-middle back muscles. Similar movements as the middle Traps.
    /// 
    /// The shoulder blades.
    /// </summary>
    [Display(GroupName = "Back", Name = "Rhomboids")]
    Rhomboids = MusculoskeletalSystem.Rhomboids, // 131072

    /// <summary>
    /// Sides of the upper chest
    /// </summary>
    [Display(GroupName = "Chest", Name = "Serratus Anterior")]
    SerratusAnterior = MusculoskeletalSystem.SerratusAnterior, // 262144

    /// <summary>
    /// Front of the leg. Lifts the ankle up.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Tibialis Anterior")]
    TibialisAnterior = MusculoskeletalSystem.TibialisAnterior, // 524288

    // Parts
    [Display(GroupName = "Legs", Name = "Glute Max")]
    GluteMax = MusculoskeletalSystem.GluteMax, // 2147483648
    //[Display(GroupName = "Legs", Name = "Glute Med")]
    //GluteMed = MusculoskeletalSystem.GluteMed, // 4294967296
    //[Display(GroupName = "Legs", Name = "Glute Min")]
    //GluteMin = MusculoskeletalSystem.GluteMin, // 8589934592
    [Display(GroupName = "Legs", Name = "Glute Med/Min")]
    GluteMedMin = MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin, // 4294967296 + 8589934592
    [Display(GroupName = "Shoulders", Name = "Front Deltoid")]
    FrontDelt = MusculoskeletalSystem.FrontDelt, // 34359738368
    [Display(GroupName = "Shoulders", Name = "Lateral Deltoid")]
    LatDelt = MusculoskeletalSystem.LatDelt, // 68719476736
    [Display(GroupName = "Shoulders", Name = "Rear Deltoid")]
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

    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | GluteMax | GluteMedMin | FrontDelt | LatDelt | RearDelt
        | Fingers | Wrists | Elbows | Shoulders | Hips | Knees | Ankles | Toes
        | PelvicFloor
}
