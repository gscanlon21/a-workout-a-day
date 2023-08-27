using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Major muscle groups of the body. We are working all of these muscle groups out for a full-body workout.
/// </summary>
[Flags]
public enum MuscleGroups : long
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
    [Display(GroupName = "Shoulders", Name = "Deltoids")]
    Deltoids = MusculoskeletalSystem.Deltoids, // 4

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
    [Display(GroupName = "Core", Name = "Spinal Erector")]
    ErectorSpinae = MusculoskeletalSystem.ErectorSpinae, // 128

    /// <summary>
    /// Hip Abductors (Lift your leg out to the side, or from a squatting position, knees falls out to the side) 
    /// - gluteus medius and minimus.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – gluteus maximus.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Glutes")]
    Glutes = MusculoskeletalSystem.Glutes, // 256

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
    /// Sides of the upper chest
    /// </summary>
    [Display(GroupName = "Legs", Name = "Tibialis Anterior")]
    TibialisAnterior = MusculoskeletalSystem.TibialisAnterior, // 524288

    // Other
    [Display(Name = "Pelvic Floor")]
    PelvicFloor = MusculoskeletalSystem.PelvicFloor, // 268435456

    [Display(Name = "Eyes")]
    Eyes = MusculoskeletalSystem.Eyes, // 536870912

    [Display(Name = "Neck")]
    Neck = MusculoskeletalSystem.Neck, // 1073741824

    // Parts
    [Display(GroupName = "Legs", Name = "Glute Max")]
    GluteMax = MusculoskeletalSystem.GluteMax, // 256 | 2147483648

    [Display(GroupName = "Legs", Name = "Glute Med/Min")]
    GluteMedMin = MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin, // 256 | 4294967296 | 8589934592

    [Display(GroupName = "Shoulders", Name = "Front Deltoid")]
    FrontDelt = MusculoskeletalSystem.FrontDelt, // 4 | 17179869184

    [Display(GroupName = "Shoulders", Name = "Lateral Deltoid")]
    LatDelt = MusculoskeletalSystem.LatDelt, // 4 | 34359738368

    [Display(GroupName = "Shoulders", Name = "Rear Deltoid")]
    RearDelt = MusculoskeletalSystem.RearDelt, // 4 | 68719476736


    // ----- Groups to work out together ------ //

    [Display(Name = "Upper Body Push")]
    UpperBodyPush = Forearms | RotatorCuffs | FrontDelt | LatDelt | Triceps | Pectorals | SerratusAnterior,

    [Display(Name = "Upper Body Pull")]
    UpperBodyPull = Forearms | RotatorCuffs | RearDelt | Biceps | LatissimusDorsi | Trapezius | Rhomboids,

    [Display(Name = "Upper Body")]
    UpperBody = Forearms | RotatorCuffs | FrontDelt | LatDelt | RearDelt | Triceps | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals,

    [Display(Name = "Lower Body")]
    LowerBody = Quadriceps | Calves | Hamstrings | HipAdductors | GluteMax | GluteMedMin,

    [Display(Name = "Full Body")]
    UpperLower = UpperBody | LowerBody,

    // ----- Common groups ------ //

    /// <summary>
    /// Muscles that help with trunk stability.
    /// </summary>
    [Display(Name = "Core")]
    Core = Abdominals | Obliques | ErectorSpinae | HipFlexors,

    /// <summary>
    /// All muscle groups. Not including pelvic floor because it shouldn't be a part of workouts.
    /// </summary>
    [Display(Name = "Full Body")]
    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | Glutes | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | Deltoids | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | PelvicFloor | Eyes | Neck
        | GluteMax | GluteMedMin | FrontDelt | LatDelt | RearDelt
}
