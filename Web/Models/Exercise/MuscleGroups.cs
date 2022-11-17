using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Major muscle groups of the body. We are working all of these muscle groups out for a full-body workout.
/// </summary>
[Flags]
public enum MuscleGroups
{
    None = 0,

    /// <summary>
    /// Stomach muscles
    /// </summary>
    [Display(GroupName = "Core", Name = "Abdominals", ShortName = "Abs")]
    Abdominals = 1 << 0, // 1

    /// <summary>
    /// Front of upper arm muscles
    /// </summary>
    [Display(GroupName = "Arms", Name = "Biceps")]
    Biceps = 1 << 1, // 2

    /// <summary>
    /// Almost-shoulder muscles
    /// </summary>
    [Display(GroupName = "Shoulders", Name = "Deltoids")]
    Deltoids = 1 << 2, // 4

    /// <summary>
    /// Chest muscles
    /// </summary>
    [Display(GroupName = "Chest", Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = 1 << 3, // 8

    /// <summary>
    /// Side muscles
    /// </summary>
    [Display(GroupName = "Core", Name = "Obliques")]
    Obliques = 1 << 4, // 16

    /// <summary>
    /// Upper back muscles
    /// </summary>
    [Display(GroupName = "Back", Name = "Trapezius", ShortName = "Traps")]
    Trapezius = 1 << 5, // 32

    /// <summary>
    /// Back muscles
    /// </summary>
    [Display(GroupName = "Back", Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = 1 << 6, // 64

    /// <summary>
    /// Spinal muscles
    /// </summary>
    [Display(GroupName = "Core", Name = "Spinal Erector")]
    ErectorSpinae = 1 << 7, // 128

    /// <summary>
    /// Hip Abductors (Lift your leg out to the side, or from a squatting position, knees falls out to the side) 
    /// - gluteus medius and minimus.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – gluteus maximus.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Glutes")]
    Glutes = 1 << 8, // 256

    /// <summary>
    /// Back of upper leg muscles.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – hamstrings (focus on biceps femoris).
    /// </summary>
    [Display(GroupName = "Legs", Name = "Hamstrings")]
    Hamstrings = 1 << 9, // 512

    /// <summary>
    /// Lower leg muscles
    /// </summary>
    [Display(GroupName = "Legs", Name = "Calves")]
    Calves = 1 << 10, // 1024

    /// <summary>
    /// Front of upper leg muscles
    /// </summary>
    [Display(GroupName = "Legs", Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = 1 << 11, // 2048

    /// <summary>
    /// Back of upper arm muscles
    /// </summary>
    [Display(GroupName = "Arms", Name = "Triceps")]
    Triceps = 1 << 12, // 4096

    /// <summary>
    /// Hip flexors (Lift your thigh upward in front of your body) 
    /// - rectus femoris, iliopsoas, sartorius, and tensor fasciae latae.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = 1 << 13, // 8192

    /// <summary>
    /// The lower-part of the arm between the hand and the elbow.
    /// </summary>
    [Display(GroupName = "Arms", Name = "Forearms")]
    Forearms = 1 << 14, // 16384

    /// <summary>
    /// The inner thigh groin muscles. Helps move the hip laterally out to the side and across the body.
    /// 
    /// From a position of hip abduction, lower your thigh to the anatomical position.
    /// </summary>
    [Display(GroupName = "Legs", Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = 1 << 15, // 32768

    /// <summary>
    /// Shoulder muscles
    /// </summary>
    [Display(GroupName = "Shoulders", Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = 1 << 16, // 65536

    /// <summary>
    /// Deep upper-middle back muscles. Similar movements as the middle Traps.
    /// 
    /// The shoulder blades.
    /// </summary>
    [Display(GroupName = "Back", Name = "Rhomboids")]
    Rhomboids = 1 << 17, // 131072

    /// <summary>
    /// Sides of the upper chest
    /// </summary>
    [Display(GroupName = "Chest", Name = "Serratus Anterior")]
    SerratusAnterior = 1 << 18, // 262144

    // ----- Groups to work out together ------ //

    [Display(Name = "Upper Body Push")]
    UpperBodyPush = Triceps | Pectorals | Deltoids | SerratusAnterior | RotatorCuffs | Forearms,

    [Display(Name = "Upper Body Pull")]
    UpperBodyPull = LatissimusDorsi | Trapezius | Rhomboids | Biceps | RotatorCuffs | Forearms,

    [Display(Name = "Upper Body")]
    UpperBody = Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | Deltoids | RotatorCuffs | SerratusAnterior,

    [Display(Name = "Major Core")]
    MajorCore = Abdominals | ErectorSpinae,

    [Display(Name = "Core")]
    Core = Abdominals | Obliques | ErectorSpinae,

    [Display(Name = "Lower Body")]
    LowerBody = Quadriceps | Calves | Hamstrings | Glutes | HipAdductors | HipFlexors,

    [Display(Name = "Full Body")]
    All = UpperBody | Core | LowerBody
}
