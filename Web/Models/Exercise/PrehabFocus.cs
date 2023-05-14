using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

[Flags]
public enum PrehabFocus
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None,

    [Display(Name = "Fingers")]
    Fingers = MusculoskeletalSystem.FingerJoints | MusculoskeletalSystem.Forearms, // 1048576

    [Display(Name = "Hands")]
    Hands = MusculoskeletalSystem.WristJoints | MusculoskeletalSystem.Forearms, // 2097152

    [Display(Name = "Arms")]
    Arms = MusculoskeletalSystem.ElbowJoints | MusculoskeletalSystem.Triceps | MusculoskeletalSystem.Biceps | MusculoskeletalSystem.Forearms, // 4194304

    [Display(Name = "Shoulders")]
    Shoulders = MusculoskeletalSystem.ShoulderJoints | MusculoskeletalSystem.Trapezius | MusculoskeletalSystem.Rhomboids | MusculoskeletalSystem.RotatorCuffs | MusculoskeletalSystem.Deltoids, // 8388608

    [Display(Name = "Core")]
    Core = MusculoskeletalSystem.Abdominals | MusculoskeletalSystem.Obliques | MusculoskeletalSystem.ErectorSpinae, // 129

    [Display(Name = "Hip")]
    Hip = MusculoskeletalSystem.HipJoints | Core | MusculoskeletalSystem.Glutes | MusculoskeletalSystem.HipAdductors | MusculoskeletalSystem.HipFlexors, // 16777216

    [Display(Name = "Legs")]
    Legs = MusculoskeletalSystem.KneeJoints | MusculoskeletalSystem.Quadriceps | MusculoskeletalSystem.Hamstrings | MusculoskeletalSystem.Calves | Ankles, // 33554432

    [Display(Name = "Feet")]
    Ankles = MusculoskeletalSystem.Calves | MusculoskeletalSystem.AnkleJoints | MusculoskeletalSystem.TibialisAnterior, // 67108864

    [Display(Name = "Lower Back")]
    LowerBack = Core | Hip | Ankles, // 129 | 16777216 | 67108864

    [Display(Name = "Upper Back")]
    UpperBack = Core | Shoulders | Ankles, // 129 | 8388608 | 67108864

    All = Fingers | Hands | Arms | Shoulders | Core | Hip | Legs | Ankles
}
