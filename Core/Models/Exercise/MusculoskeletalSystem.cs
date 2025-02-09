using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Major muscle groups of the body. We are working all of these muscle groups out for a full-body workout.
/// </summary>
[Flags]
public enum MusculoskeletalSystem : long
{
    [Display(Name = "None")]
    None = 0,


    // ----- Muscles ------ //

    /// <summary>
    /// Stomach muscles
    /// </summary>
    [Display(Name = "Abdominals", ShortName = "Abs")]
    Abdominals = 1 << 0, // 1

    /// <summary>
    /// Front of upper arm muscles
    /// </summary>
    [Display(Name = "Biceps")]
    Biceps = 1 << 1, // 2

    /// <summary>
    /// Almost-shoulder muscles
    /// </summary>
    //[Display(GroupName = "Deltoids", Name = "Deltoids")]
    //Deltoids = 1 << 2, // 4

    /// <summary>
    /// Chest muscles
    /// </summary>
    [Display(Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = 1 << 3, // 8

    /// <summary>
    /// Side muscles
    /// </summary>
    [Display(Name = "Obliques")]
    Obliques = 1 << 4, // 16

    /// <summary>
    /// Upper back muscles
    /// </summary>
    [Display(Name = "Trapezius", ShortName = "Traps")]
    Trapezius = 1 << 5, // 32

    /// <summary>
    /// Back muscles
    /// </summary>
    [Display(Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = 1 << 6, // 64

    /// <summary>
    /// Spinal muscles
    /// </summary>
    [Display(Name = "Spinal Erectors")]
    ErectorSpinae = 1 << 7, // 128

    /// <summary>
    /// Hip Abductors (Lift your leg out to the side, or from a squatting position, knees falls out to the side) 
    /// - gluteus medius and minimus.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – gluteus maximus.
    /// </summary>
    //[Display(GroupName = "Glutes", Name = "Glutes")]
    //Glutes = 1 << 8, // 256

    /// <summary>
    /// Back of upper leg muscles.
    /// 
    /// Hip Extensors (From anatomical position, lift your thigh behind you) 
    /// – hamstrings (focus on biceps femoris).
    /// </summary>
    [Display(Name = "Hamstrings")]
    Hamstrings = 1 << 9, // 512

    /// <summary>
    /// Lower leg muscles
    /// </summary>
    [Display(Name = "Calves")]
    Calves = 1 << 10, // 1024

    /// <summary>
    /// Front of upper leg muscles
    /// </summary>
    [Display(Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = 1 << 11, // 2048

    /// <summary>
    /// Back of upper arm muscles
    /// </summary>
    [Display(Name = "Triceps")]
    Triceps = 1 << 12, // 4096

    /// <summary>
    /// Hip flexors (Lift your thigh upward in front of your body) 
    /// - rectus femoris, iliopsoas, sartorius, and tensor fasciae latae.
    /// 
    /// Stretched by extending your leg behind you. Sitting shortens the hip flexors.
    /// </summary>
    [Display(Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = 1 << 13, // 8192

    /// <summary>
    /// The lower-part of the arm between the hand and the elbow.
    /// </summary>
    [Display(Name = "Forearms")]
    Forearms = 1 << 14, // 16384

    /// <summary>
    /// The inner thigh groin muscles. Helps move the hip laterally out to the side and across the body.
    /// 
    /// From a position of hip abduction, lower your thigh to the anatomical position.
    /// </summary>
    [Display(Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = 1 << 15, // 32768

    /// <summary>
    /// Shoulder muscles
    /// </summary>
    [Display(Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = 1 << 16, // 65536

    /// <summary>
    /// Deep upper-middle back muscles. Similar movements as the middle Traps.
    /// 
    /// The shoulder blades.
    /// </summary>
    [Display(Name = "Rhomboids")]
    Rhomboids = 1 << 17, // 131072

    /// <summary>
    /// Sides of the upper chest
    /// </summary>
    [Display(Name = "Serratus Anterior")]
    SerratusAnterior = 1 << 18, // 262144

    /// <summary>
    /// Front of the leg. Lifts the ankle up.
    /// </summary>
    [Display(Name = "Tibialis Anterior")]
    TibialisAnterior = 1 << 19, // 524288


    // ----- Joints ------ //

    [Display(Name = "Finger Joints", ShortName = "Fingers")]
    FingerJoints = 1 << 20, // 1048576

    [Display(Name = "Wrist Joints", ShortName = "Wrists")]
    WristJoints = 1 << 21, // 2097152

    [Display(Name = "Elbow Joints", ShortName = "Elbows")]
    ElbowJoints = 1 << 22, // 4194304

    [Display(Name = "Shoulder Joints", ShortName = "Shoulders")]
    ShoulderJoints = 1 << 23, // 8388608

    [Display(Name = "Hip Joints", ShortName = "Hips")]
    HipJoints = 1 << 24, // 16777216

    /// <summary>
    /// https://healthonline.washington.edu/sites/default/files/record_pdfs/Isometric-Exercises-Patellar-Tendinopathy.pdf
    /// </summary>
    [Display(Name = "Knee Joints", ShortName = "Knees")]
    KneeJoints = 1 << 25, // 33554432

    [Display(Name = "Ankle Joints", ShortName = "Ankles")]
    AnkleJoints = 1 << 26, // 67108864

    [Display(Name = "Toe Joints", ShortName = "Toes")]
    ToeJoints = 1 << 27, // 134217728


    // ----- Other ------ //

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

    [Display(Name = "Mind")]
    Mind = 1L << 42, // 4398046511104

    [Display(Name = "Face")]
    Face = 1L << 43, // 8796093022208

    [Display(Name = "Intercostal")]
    Intercostal = 1L << 44, // 17592186044416

    [Display(Name = "Thoracic Spine")]
    ThoracicSpine = 1L << 45, // 35184372088832

    [Display(Name = "IT Band")]
    ITBand = 1L << 46, // 70368744177664

    [Display(Name = "Skin")]
    Skin = 1L << 47, // 140737488355328

    [Display(Name = "Heart")]
    Heart = 1L << 48, // 281474976710656

    [Display(Name = "Suboccipitals")]
    Suboccipitals = 1L << 49, // 562949953421312


    // ----- Muscle parts ------ //

    [Display(GroupName = "Glutes", Name = "Glute Max")]
    GluteMax = 1L << 31, // 2147483648

    [Display(GroupName = "Glutes", Name = "Glute Med")]
    GluteMed = 1L << 32, // 4294967296

    [Display(GroupName = "Glutes", Name = "Glute Min")]
    GluteMin = 1L << 33, // 8589934592

    [Display(GroupName = "Glutes", Name = "Glute Med/Min")]
    GluteMedMin = GluteMed | GluteMin, // 12884901888

    [Display(GroupName = "Glutes", Name = "Glutes")]
    Glutes = GluteMax | GluteMed | GluteMin, // 15032385536

    [Display(GroupName = "Deltoids", Name = "Front Deltoid")]
    FrontDelt = 1L << 35, // 34359738368

    [Display(GroupName = "Deltoids", Name = "Lateral Deltoid")]
    LatDelt = 1L << 36, // 68719476736

    [Display(GroupName = "Deltoids", Name = "Rear Deltoid")]
    RearDelt = 1L << 37, // 137438953472

    [Display(GroupName = "Deltoids", Name = "Deltoids")]
    Deltoids = FrontDelt | LatDelt | RearDelt, // 240518168576

    // ----- Groups to work out together ------ //

    [Display(Name = "Upper Body Push", Order = 1)]
    UpperBodyPush = Forearms | RotatorCuffs | FrontDelt | LatDelt | Triceps | Pectorals | SerratusAnterior,

    [Display(Name = "Upper Body Pull", Order = 1)]
    UpperBodyPull = Forearms | RotatorCuffs | RearDelt | Biceps | LatissimusDorsi | Trapezius | Rhomboids,

    [Display(Name = "Upper Body", Order = 1)]
    UpperBody = Forearms | RotatorCuffs | FrontDelt | LatDelt | RearDelt | Triceps | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | SerratusAnterior,

    [Display(Name = "Lower Body", Order = 1)]
    LowerBody = Quadriceps | Calves | Hamstrings | HipAdductors | GluteMax | GluteMed | GluteMin | TibialisAnterior | HipFlexors,

    [Display(Name = "Full Body", Order = 1)]
    UpperLower = UpperBody | LowerBody,

    // ----- Common groups ------ //

    /// <summary>
    /// Muscles that help with trunk stability.
    /// </summary>
    [Display(Name = "Core", Order = 2)]
    Core = Abdominals | Obliques | ErectorSpinae,

    /// <summary>
    /// All muscle groups.
    /// </summary>
    [Display(Name = "Full Body")]
    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | PelvicFloor | Eyes | Neck | Nose | VocalCords | Diaphragm | Throat | Mind | Face | Intercostal | ThoracicSpine | ITBand | Skin | Heart | Suboccipitals
        | GluteMax | GluteMed | GluteMin | FrontDelt | LatDelt | RearDelt
}

public static class MuscleGroupExtensions
{
    public static IList<MusculoskeletalSystem> Core()
    {
        return [MusculoskeletalSystem.Abdominals, MusculoskeletalSystem.Obliques, MusculoskeletalSystem.ErectorSpinae];
    }

    public static IList<MusculoskeletalSystem> Lower()
    {
        return [MusculoskeletalSystem.Quadriceps, MusculoskeletalSystem.Calves, MusculoskeletalSystem.Hamstrings, MusculoskeletalSystem.HipAdductors, MusculoskeletalSystem.HipFlexors, MusculoskeletalSystem.GluteMax, MusculoskeletalSystem.GluteMedMin, MusculoskeletalSystem.TibialisAnterior];
    }

    public static IList<MusculoskeletalSystem> Upper()
    {
        return [MusculoskeletalSystem.Forearms, MusculoskeletalSystem.RotatorCuffs, MusculoskeletalSystem.FrontDelt, MusculoskeletalSystem.LatDelt, MusculoskeletalSystem.RearDelt, MusculoskeletalSystem.Triceps, MusculoskeletalSystem.Biceps, MusculoskeletalSystem.LatissimusDorsi, MusculoskeletalSystem.Trapezius, MusculoskeletalSystem.Rhomboids, MusculoskeletalSystem.Pectorals, MusculoskeletalSystem.SerratusAnterior];
    }

    public static IList<MusculoskeletalSystem> UpperPush()
    {
        return [MusculoskeletalSystem.Forearms, MusculoskeletalSystem.RotatorCuffs, MusculoskeletalSystem.FrontDelt, MusculoskeletalSystem.LatDelt, MusculoskeletalSystem.Triceps, MusculoskeletalSystem.Pectorals, MusculoskeletalSystem.SerratusAnterior];
    }

    public static IList<MusculoskeletalSystem> UpperPull()
    {
        return [MusculoskeletalSystem.Forearms, MusculoskeletalSystem.RotatorCuffs, MusculoskeletalSystem.RearDelt, MusculoskeletalSystem.Biceps, MusculoskeletalSystem.LatissimusDorsi, MusculoskeletalSystem.Trapezius, MusculoskeletalSystem.Rhomboids];
    }

    public static IList<MusculoskeletalSystem> UpperLower()
    {
        return [.. Upper(), .. Lower()];
    }

    public static IList<MusculoskeletalSystem> All()
    {
        return
        [
            MusculoskeletalSystem.Abdominals, MusculoskeletalSystem.Obliques, MusculoskeletalSystem.ErectorSpinae, MusculoskeletalSystem.Quadriceps, MusculoskeletalSystem.Calves, MusculoskeletalSystem.Hamstrings, MusculoskeletalSystem.HipAdductors, MusculoskeletalSystem.HipFlexors, MusculoskeletalSystem.Triceps, MusculoskeletalSystem.Forearms,
            MusculoskeletalSystem.Biceps, MusculoskeletalSystem.LatissimusDorsi, MusculoskeletalSystem.Trapezius, MusculoskeletalSystem.Rhomboids, MusculoskeletalSystem.Pectorals, MusculoskeletalSystem.RotatorCuffs, MusculoskeletalSystem.SerratusAnterior, MusculoskeletalSystem.TibialisAnterior,
            MusculoskeletalSystem.PelvicFloor, MusculoskeletalSystem.Nose, MusculoskeletalSystem.VocalCords, MusculoskeletalSystem.Eyes, MusculoskeletalSystem.Neck, MusculoskeletalSystem.Diaphragm, MusculoskeletalSystem.Throat, MusculoskeletalSystem.Mind, MusculoskeletalSystem.Face, MusculoskeletalSystem.Intercostal, MusculoskeletalSystem.ThoracicSpine, MusculoskeletalSystem.ITBand, MusculoskeletalSystem.Skin,
            MusculoskeletalSystem.GluteMax, MusculoskeletalSystem.GluteMed, MusculoskeletalSystem.GluteMin, MusculoskeletalSystem.FrontDelt, MusculoskeletalSystem.LatDelt, MusculoskeletalSystem.RearDelt, MusculoskeletalSystem.Heart, MusculoskeletalSystem.Suboccipitals
        ];
    }
}