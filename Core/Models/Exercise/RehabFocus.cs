using Core.Code.Attributes;
using Core.Models.Exercise.Skills;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Major muscle groups of the body that can be targeted for injury rehabilitation exercises.
/// </summary>
[Flags]
public enum RehabFocus : long
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None,


    // ----- Muscles ------ //

    [Display(Name = "Core")]
    Core = MusculoskeletalSystem.Abdominals | MusculoskeletalSystem.Obliques | MusculoskeletalSystem.ErectorSpinae,

    [Display(Name = "Glutes")]
    Glutes = MusculoskeletalSystem.GluteMax | MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin,

    [Display(Name = "Hip Flexors")]
    HipFlexors = MusculoskeletalSystem.HipFlexors,


    // ----- Joints ------ //

    [Display(GroupName = "Joints", Name = "Fingers")]
    Fingers = MusculoskeletalSystem.FingerJoints,

    [Display(GroupName = "Joints", Name = "Wrists")]
    Wrists = MusculoskeletalSystem.WristJoints,

    [Display(GroupName = "Joints", Name = "Elbows")]
    Elbows = MusculoskeletalSystem.ElbowJoints,

    [Display(GroupName = "Joints", Name = "Shoulders")]
    Shoulders = MusculoskeletalSystem.ShoulderJoints | MusculoskeletalSystem.RotatorCuffs | MusculoskeletalSystem.SerratusAnterior
        | MusculoskeletalSystem.FrontDelt | MusculoskeletalSystem.LatDelt | MusculoskeletalSystem.RearDelt,

    [Display(GroupName = "Joints", Name = "Hips")]
    Hips = MusculoskeletalSystem.HipJoints | MusculoskeletalSystem.HipFlexors | MusculoskeletalSystem.HipAdductors,

    [Display(GroupName = "Joints", Name = "Knees")]
    Knees = MusculoskeletalSystem.KneeJoints,

    [Display(GroupName = "Joints", Name = "Ankles")]
    Ankles = MusculoskeletalSystem.AnkleJoints,

    [Display(GroupName = "Joints", Name = "Toes")]
    Toes = MusculoskeletalSystem.ToeJoints,


    // ----- Body Parts ----- //

    [Display(Name = "Head")]
    Head = MusculoskeletalSystem.Suboccipitals,

    [Display(Name = "Mouth")]
    Mouth = MusculoskeletalSystem.Mouth | MusculoskeletalSystem.Tongue,

    [Display(GroupName = "Spine", Name = "Neck"), Skills<CervicalSkills>()]
    Neck = MusculoskeletalSystem.CervicalSpine | MusculoskeletalSystem.Scalenes,

    [Display(GroupName = "Back", Name = "Upper Back"), Skills<ThoracicSkills>()]
    UpperBack = MusculoskeletalSystem.ThoracicSpine | MusculoskeletalSystem.UpperTraps | MusculoskeletalSystem.LowerTraps | MusculoskeletalSystem.Rhomboids,

    [Display(GroupName = "Back", Name = "Lower Back"), Skills<LumbarSkills>()]
    LowerBack = MusculoskeletalSystem.LumbarSpine | MusculoskeletalSystem.LatissimusDorsi | MusculoskeletalSystem.ErectorSpinae,

    [Display(Name = "Chest")]
    Chest = MusculoskeletalSystem.Pectorals,

    [Display(Name = "Arms")]
    Arms = MusculoskeletalSystem.Forearms | MusculoskeletalSystem.Triceps | MusculoskeletalSystem.Biceps,

    [Display(Name = "Hands")]
    Hands = MusculoskeletalSystem.Hands,

    [Display(GroupName = "Legs", Name = "Upper Legs")]
    UpperLegs = MusculoskeletalSystem.Hamstrings | MusculoskeletalSystem.Quadriceps | MusculoskeletalSystem.ITBand,

    [Display(GroupName = "Legs", Name = "Lower Legs")]
    LowerLegs = MusculoskeletalSystem.Calves | MusculoskeletalSystem.TibialisAnterior | MusculoskeletalSystem.Peroneals,

    [Display(Name = "Feet")]
    Feet = MusculoskeletalSystem.Feet,


    // ----- Other ------ //

    [Display(Name = "Pelvic Floor")]
    PelvicFloor = MusculoskeletalSystem.PelvicFloor,

    [Display(Name = "Eyes"), Skills<VisualSkills>()]
    Eyes = MusculoskeletalSystem.Eyes,

    [Display(Name = "Mind")]
    Mind = MusculoskeletalSystem.Mind,

    [Display(Name = "Face")]
    Face = MusculoskeletalSystem.Face,

    [Display(Name = "Skin")]
    Skin = MusculoskeletalSystem.Skin,

    [Display(Name = "Heart")]
    Heart = MusculoskeletalSystem.Heart,

    [Display(Name = "Voice"), Skills<VocalSkills>()]
    Voice = MusculoskeletalSystem.VocalCords | MusculoskeletalSystem.Tongue,

    [Display(Name = "Breathing")]
    Breathing = MusculoskeletalSystem.Nose | MusculoskeletalSystem.Throat | MusculoskeletalSystem.Diaphragm | MusculoskeletalSystem.Intercostals,


    All = Core | Glutes | HipFlexors
        | Fingers | Wrists | Elbows | Shoulders | Hips | Knees | Ankles | Toes | Hands | Feet
        | Head | Neck | Mouth | UpperBack | LowerBack | Chest | Arms | UpperLegs | LowerLegs
        | PelvicFloor | Eyes | Voice | Breathing | Mind | Face | Skin | Heart
}
