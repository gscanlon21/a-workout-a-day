using Core.Code.Attributes;
using Core.Models.Exercise.Skills;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Major muscle groups of the body that can be targeted for rehabilitation exercises.
/// </summary>
[Flags]
public enum RehabFocus : long
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None,


    // ----- Muscles ------ //

    [Display(Name = "Abdominals", ShortName = "Abs")]
    Abdominals = MusculoskeletalSystem.Abdominals, // 1

    [Display(Name = "Biceps")]
    Biceps = MusculoskeletalSystem.Biceps, // 2

    //[Display(GroupName = "Deltoids", Name = "Deltoids")]
    //Deltoids = MusculoskeletalSystem.Deltoids, // 4

    [Display(Name = "Pectorals", ShortName = "Pecs")]
    Pectorals = MusculoskeletalSystem.Pectorals, // 8

    [Display(Name = "Obliques")]
    Obliques = MusculoskeletalSystem.Obliques, // 16

    //[Display(Name = "Trapezius", ShortName = "Traps")]
    //Trapezius = MusculoskeletalSystem.Trapezius, // 32

    [Display(Name = "Latissimus Dorsi", ShortName = "Lats")]
    LatissimusDorsi = MusculoskeletalSystem.LatissimusDorsi, // 64

    [Display(Name = "Spinal Erectors")]
    ErectorSpinae = MusculoskeletalSystem.ErectorSpinae, // 128

    //[Display(GroupName = "Glutes", Name = "Glutes")]
    //Glutes = MusculoskeletalSystem.Glutes, // 256

    [Display(Name = "Hamstrings")]
    Hamstrings = MusculoskeletalSystem.Hamstrings, // 512

    [Display(Name = "Calves")]
    Calves = MusculoskeletalSystem.Calves, // 1024

    [Display(Name = "Quadriceps", ShortName = "Quads")]
    Quadriceps = MusculoskeletalSystem.Quadriceps, // 2048

    [Display(Name = "Triceps")]
    Triceps = MusculoskeletalSystem.Triceps, // 4096

    [Display(Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
    HipFlexors = MusculoskeletalSystem.HipFlexors, // 8192

    [Display(Name = "Forearms")]
    Forearms = MusculoskeletalSystem.Forearms, // 16384

    [Display(Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
    HipAdductors = MusculoskeletalSystem.HipAdductors, // 32768

    [Display(Name = "Rotator Cuffs", Description = "Shoulders")]
    RotatorCuffs = MusculoskeletalSystem.RotatorCuffs, // 65536

    [Display(Name = "Rhomboids")]
    Rhomboids = MusculoskeletalSystem.Rhomboids, // 131072

    [Display(Name = "Serratus Anterior")]
    SerratusAnterior = MusculoskeletalSystem.SerratusAnterior, // 262144

    [Display(Name = "Tibialis Anterior")]
    TibialisAnterior = MusculoskeletalSystem.TibialisAnterior, // 524288


    // ----- Muscle parts ------ //

    [Display(GroupName = "Glutes", Name = "Glute Max")]
    GluteMax = MusculoskeletalSystem.GluteMax, // 2147483648

    //[Display(GroupName = "Glutes", Name = "Glute Med")]
    //GluteMed = MuscleGroups.GluteMed, // 4294967296

    //[Display(GroupName = "Glutes", Name = "Glute Min")]
    //GluteMin = MuscleGroups.GluteMin, // 8589934592

    [Display(GroupName = "Glutes", Name = "Glute Med/Min")]
    GluteMedMin = MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin, // 12884901888

    [Display(GroupName = "Deltoids", Name = "Front Deltoid", ShortName = "Front Delt")]
    FrontDelt = MusculoskeletalSystem.FrontDelt, // 34359738368

    [Display(GroupName = "Deltoids", Name = "Lateral Deltoid", ShortName = "Lateral Delt")]
    LatDelt = MusculoskeletalSystem.LatDelt, // 68719476736

    [Display(GroupName = "Deltoids", Name = "Rear Deltoid", ShortName = "Rear Delt")]
    RearDelt = MusculoskeletalSystem.RearDelt, // 137438953472

    [Display(GroupName = "Trapezius", Name = "Upper Trapezius", ShortName = "Upper Traps")]
    UpperTraps = MusculoskeletalSystem.UpperTraps, // 36028797018963968

    [Display(GroupName = "Trapezius", Name = "Lower Trapezius", ShortName = "Lower Traps")]
    LowerTraps = MusculoskeletalSystem.LowerTraps, // 72057594037927936


    // ----- Joints ------ //

    [Display(GroupName = "Joints", Name = "Fingers")]
    Fingers = MusculoskeletalSystem.FingerJoints, // 1048576

    [Display(GroupName = "Joints", Name = "Wrists")]
    Wrists = MusculoskeletalSystem.WristJoints, // 2097152

    [Display(GroupName = "Joints", Name = "Elbows")]
    Elbows = MusculoskeletalSystem.ElbowJoints, // 4194304

    [Display(GroupName = "Joints", Name = "Shoulders")]
    Shoulders = MusculoskeletalSystem.ShoulderJoints, // 8388608

    [Display(GroupName = "Joints", Name = "Hips")]
    Hips = MusculoskeletalSystem.HipJoints, // 16777216

    [Display(GroupName = "Joints", Name = "Knees")]
    Knees = MusculoskeletalSystem.KneeJoints, // 33554432

    [Display(GroupName = "Joints", Name = "Ankles")]
    Ankles = MusculoskeletalSystem.AnkleJoints, // 67108864

    [Display(GroupName = "Joints", Name = "Toes")]
    Toes = MusculoskeletalSystem.ToeJoints, // 134217728


    // ----- Other ------ //

    [Display(Name = "Pelvic Floor")]
    PelvicFloor = MusculoskeletalSystem.PelvicFloor, // 268435456

    [Display(Name = "Eyes"), Skills<VisualSkills>()]
    Eyes = MusculoskeletalSystem.Eyes, // 536870912

    [Display(GroupName = "Spine", Name = "Cervical Spine", ShortName = "Neck"), Skills<CervicalSkills>()]
    CervicalSpine = MusculoskeletalSystem.CervicalSpine, // 1073741824

    [Display(Name = "Nose")]
    Nose = MusculoskeletalSystem.Nose, // 274877906944

    [Display(Name = "Vocal Cords")]
    VocalCords = MusculoskeletalSystem.VocalCords, // 549755813888

    [Display(Name = "Diaphragm")]
    Diaphragm = MusculoskeletalSystem.Diaphragm, // 1099511627776

    [Display(Name = "Throat")]
    Throat = MusculoskeletalSystem.Throat, // 2199023255552

    [Display(Name = "Mind")]
    Mind = MusculoskeletalSystem.Mind, // 4398046511104

    [Display(Name = "Face")]
    Face = MusculoskeletalSystem.Face, // 8796093022208

    [Display(Name = "Intercostals")]
    Intercostals = MusculoskeletalSystem.Intercostals, // 17592186044416

    [Display(GroupName = "Spine", Name = "Thoracic Spine"), Skills<ThoracicSkills>()]
    ThoracicSpine = MusculoskeletalSystem.ThoracicSpine, // 35184372088832

    [Display(Name = "IT Band")]
    ITBand = MusculoskeletalSystem.ITBand, // 70368744177664

    [Display(Name = "Skin")]
    Skin = MusculoskeletalSystem.Skin, // 140737488355328

    [Display(Name = "Heart")]
    Heart = MusculoskeletalSystem.Heart, // 281474976710656

    [Display(Name = "Suboccipitals")]
    Suboccipitals = MusculoskeletalSystem.Suboccipitals, // 562949953421312

    [Display(Name = "Mouth"), Skills<VocalSkills>()]
    Mouth = MusculoskeletalSystem.Mouth, // 1125899906842624

    [Display(Name = "Tongue"), Skills<VocalSkills>()]
    Tongue = MusculoskeletalSystem.Tongue, // 2251799813685248

    [Display(GroupName = "Spine", Name = "Lumbar Spine"), Skills<LumbarSkills>()]
    LumbarSpine = MusculoskeletalSystem.LumbarSpine, // 4503599627370496

    [Display(Name = "Hands")]
    Hands = MusculoskeletalSystem.Hands, // 9007199254740992

    [Display(Name = "Feet")]
    Feet = MusculoskeletalSystem.Feet, // 18014398509481984

    [Display(Name = "Peroneals")]
    Peroneals = MusculoskeletalSystem.Peroneals, // 144115188075855872


    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Rhomboids | Pectorals | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | GluteMax | GluteMedMin | FrontDelt | LatDelt | RearDelt | UpperTraps | LowerTraps
        | Fingers | Wrists | Elbows | Shoulders | Hips | Knees | Ankles | Toes | Hands | Feet
        | PelvicFloor | Eyes | CervicalSpine | Nose | VocalCords | Diaphragm | Throat | Mind | Face | Intercostals | ThoracicSpine | ITBand | Skin | Heart | Suboccipitals | Mouth | Tongue | LumbarSpine | Peroneals
}
