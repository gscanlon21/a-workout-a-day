﻿using System.ComponentModel.DataAnnotations;

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
    [Display(GroupName = "Deltoids", Name = "Deltoids")]
    Deltoids = RearDelt | LatDelt | FrontDelt, // 240518168576

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
    [Display(GroupName = "Glutes", Name = "Glutes")]
    Glutes = GluteMax | GluteMed | GluteMin, // 15032385536

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
    /// Sides of the upper chest.
    /// 
    /// The serratus anterior is trained indirectly in all anterior deltoid exercises. 
    /// It works to some extent in the select few chest exercises that involve scapular protraction.
    /// </summary>
    [Display(Name = "Serratus Anterior")]
    SerratusAnterior = MusculoskeletalSystem.SerratusAnterior, // 262144

    /// <summary>
    /// Sides of the upper chest
    /// </summary>
    [Display(Name = "Tibialis Anterior")]
    TibialisAnterior = MusculoskeletalSystem.TibialisAnterior, // 524288

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

    [Display(Name = "Throat")]
    Throat = MusculoskeletalSystem.Throat, // 2199023255552

    [Display(Name = "Mind")]
    Mind = MusculoskeletalSystem.Mind, // 4398046511104

    [Display(Name = "Face")]
    Face = MusculoskeletalSystem.Face, // 8796093022208

    [Display(Name = "Intercostal")]
    Intercostal = MusculoskeletalSystem.Intercostal, // 17592186044416

    [Display(Name = "Thoracic Spine")]
    ThoracicSpine = MusculoskeletalSystem.ThoracicSpine, // 35184372088832

    [Display(Name = "IT Band")]
    ITBand = MusculoskeletalSystem.ITBand, // 70368744177664

    // Parts
    [Display(GroupName = "Glutes", Name = "Glute Max")]
    GluteMax = MusculoskeletalSystem.GluteMax, // 2147483648

    [Display(GroupName = "Glutes", Name = "Glute Med")]
    GluteMed = MusculoskeletalSystem.GluteMed, // 4294967296

    [Display(GroupName = "Glutes", Name = "Glute Min")]
    GluteMin = MusculoskeletalSystem.GluteMin, // 8589934592

    [Display(GroupName = "Glutes", Name = "Glute Med/Min")]
    GluteMedMin = GluteMed | GluteMin, // 12884901888

    [Display(GroupName = "Deltoids", Name = "Front Deltoid")]
    FrontDelt = MusculoskeletalSystem.FrontDelt, // 34359738368

    [Display(GroupName = "Deltoids", Name = "Lateral Deltoid")]
    LatDelt = MusculoskeletalSystem.LatDelt, // 68719476736

    [Display(GroupName = "Deltoids", Name = "Rear Deltoid")]
    RearDelt = MusculoskeletalSystem.RearDelt, // 137438953472


    // ----- Groups to work out together ------ //

    [Display(Name = "Upper Body Push", Order = 1)]
    UpperBodyPush = Forearms | RotatorCuffs | FrontDelt | LatDelt | Triceps | Pectorals | SerratusAnterior,

    [Display(Name = "Upper Body Pull", Order = 1)]
    UpperBodyPull = Forearms | RotatorCuffs | RearDelt | Biceps | LatissimusDorsi | Trapezius | Rhomboids,

    [Display(Name = "Upper Body", Order = 1)]
    UpperBody = Forearms | RotatorCuffs | FrontDelt | LatDelt | RearDelt | Triceps | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | SerratusAnterior,

    [Display(Name = "Lower Body", Order = 1)]
    LowerBody = Quadriceps | Calves | Hamstrings | HipAdductors | GluteMax | GluteMed | GluteMin | TibialisAnterior,

    [Display(Name = "Full Body", Order = 1)]
    UpperLower = UpperBody | LowerBody,

    // ----- Common groups ------ //

    /// <summary>
    /// Muscles that help with trunk stability.
    /// </summary>
    [Display(Name = "Core", Order = 2)]
    Core = Abdominals | Obliques | ErectorSpinae | HipFlexors,

    /// <summary>
    /// All muscle groups.
    /// </summary>
    [Display(Name = "Full Body")]
    All = Abdominals | Obliques | ErectorSpinae | Quadriceps | Calves | Hamstrings | HipAdductors | HipFlexors | Triceps | Forearms | Biceps | LatissimusDorsi | Trapezius | Rhomboids | Pectorals | RotatorCuffs | SerratusAnterior | TibialisAnterior
        | PelvicFloor | Eyes | Neck | Nose | VocalCords | Diaphragm | Throat | Mind | Face | Intercostal | ThoracicSpine | ITBand
        | GluteMax | GluteMed | GluteMin | FrontDelt | LatDelt | RearDelt
}

public static class MuscleGroupExtensions
{
    public static IList<MuscleGroups> Core()
    {
        return [MuscleGroups.Abdominals, MuscleGroups.Obliques, MuscleGroups.ErectorSpinae, MuscleGroups.HipFlexors];
    }

    public static IList<MuscleGroups> Lower()
    {
        return [MuscleGroups.Quadriceps, MuscleGroups.Calves, MuscleGroups.Hamstrings, MuscleGroups.HipAdductors, MuscleGroups.GluteMax, MuscleGroups.GluteMedMin, MuscleGroups.TibialisAnterior];
    }

    public static IList<MuscleGroups> Upper()
    {
        return [MuscleGroups.Forearms, MuscleGroups.RotatorCuffs, MuscleGroups.FrontDelt, MuscleGroups.LatDelt, MuscleGroups.RearDelt, MuscleGroups.Triceps, MuscleGroups.Biceps, MuscleGroups.LatissimusDorsi, MuscleGroups.Trapezius, MuscleGroups.Rhomboids, MuscleGroups.Pectorals, MuscleGroups.SerratusAnterior];
    }

    public static IList<MuscleGroups> UpperPush()
    {
        return [MuscleGroups.Forearms, MuscleGroups.RotatorCuffs, MuscleGroups.FrontDelt, MuscleGroups.LatDelt, MuscleGroups.Triceps, MuscleGroups.Pectorals, MuscleGroups.SerratusAnterior];
    }

    public static IList<MuscleGroups> UpperPull()
    {
        return [MuscleGroups.Forearms, MuscleGroups.RotatorCuffs, MuscleGroups.RearDelt, MuscleGroups.Biceps, MuscleGroups.LatissimusDorsi, MuscleGroups.Trapezius, MuscleGroups.Rhomboids];
    }

    public static IList<MuscleGroups> UpperLower()
    {
        return [.. Upper(), .. Lower()];
    }

    public static IList<MuscleGroups> All()
    {
        return
        [
            MuscleGroups.Abdominals, MuscleGroups.Obliques, MuscleGroups.ErectorSpinae, MuscleGroups.Quadriceps, MuscleGroups.Calves, MuscleGroups.Hamstrings, MuscleGroups.HipAdductors, MuscleGroups.HipFlexors, MuscleGroups.Triceps, MuscleGroups.Forearms,
            MuscleGroups.Biceps, MuscleGroups.LatissimusDorsi, MuscleGroups.Trapezius, MuscleGroups.Rhomboids, MuscleGroups.Pectorals, MuscleGroups.RotatorCuffs, MuscleGroups.SerratusAnterior, MuscleGroups.TibialisAnterior,
            MuscleGroups.PelvicFloor, MuscleGroups.Nose, MuscleGroups.VocalCords, MuscleGroups.Eyes, MuscleGroups.Neck, MuscleGroups.Diaphragm, MuscleGroups.Throat, MuscleGroups.Mind, MuscleGroups.Face, MuscleGroups.Intercostal, MuscleGroups.ThoracicSpine, MuscleGroups.ITBand,
            MuscleGroups.GluteMax, MuscleGroups.GluteMed, MuscleGroups.GluteMin, MuscleGroups.FrontDelt, MuscleGroups.LatDelt, MuscleGroups.RearDelt
        ];
    }
}