using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

public enum Joints
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None, // 0

    [Display(Name = "Finger Joints")]
    FingerJoints = MusculoskeletalSystem.FingerJoints, // 1048576

    [Display(Name = "Wrist Joints")]
    WristJoints = MusculoskeletalSystem.WristJoints, // 2097152

    [Display(Name = "Elbow Joints")]
    ElbowJoints = MusculoskeletalSystem.ElbowJoints, // 4194304

    [Display(Name = "Shoulder Joints")]
    ShoulderJoints = MusculoskeletalSystem.ShoulderJoints, // 8388608

    [Display(Name = "Hip Joints")]
    HipJoints = MusculoskeletalSystem.HipJoints, // 16777216

    [Display(Name = "Knee Joints")]
    KneeJoints = MusculoskeletalSystem.KneeJoints, // 33554432

    [Display(Name = "Ankle Joints")]
    AnkleJoints = MusculoskeletalSystem.AnkleJoints, // 67108864

    [Display(Name = "Toe Joints")]
    ToeJoints = MusculoskeletalSystem.ToeJoints, // 134217728

    All = FingerJoints | WristJoints | ElbowJoints | ShoulderJoints | HipJoints | KneeJoints | AnkleJoints | ToeJoints
}
