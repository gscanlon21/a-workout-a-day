using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

[Flags]
public enum PrehabFocus
{
    [Display(Name = "None")]
    None = MusculoskeletalSystem.None,

    [Display(Name = "Fingers")]
    // 1048576 | 16384
    Fingers = MusculoskeletalSystem.FingerJoints 
        | MusculoskeletalSystem.Forearms, // Forearms hewlp flex your fingers

    [Display(Name = "Wrists")]
    // 2097152 | 16384
    Wrists = MusculoskeletalSystem.WristJoints 
        | MusculoskeletalSystem.Forearms, // Forearms help bend your wrists

    [Display(Name = "Elbows")]
    // 4194304 | 4096 | 2 | 16384
    Elbows = MusculoskeletalSystem.ElbowJoints 
        | MusculoskeletalSystem.Triceps // Triceps help straighten your elbows
        | MusculoskeletalSystem.Biceps // Biceps help bend your elbows
        | MusculoskeletalSystem.Forearms, // Forearms help bend/straighten your elbows
    
    [Display(Name = "Shoulders")]
    // 8388608 | 32 | 131072 | 65536 | 262144 | 4 | 8
    Shoulders = MusculoskeletalSystem.ShoulderJoints 
        | MusculoskeletalSystem.Trapezius // Traps help maintain proper shoulder alignment
        | MusculoskeletalSystem.Rhomboids // Rhomboids help maintain proper scapular alignment
        | MusculoskeletalSystem.RotatorCuffs // These help maintain proper scapular alignment
        | MusculoskeletalSystem.SerratusAnterior // These help maintain proper shoulder alignment
        | MusculoskeletalSystem.Deltoids // Delts help maintain proper shoulder alignment
        | MusculoskeletalSystem.Pectorals, // Pecs help maintain proper shoulder alignment

    [Display(Name = "Hip")]
    // 16777216 | 256 | 32768 | 8192
    Hip = MusculoskeletalSystem.HipJoints 
        | MusculoskeletalSystem.Glutes // Glutes help with hip straightening
        | MusculoskeletalSystem.HipAdductors // Glutes help with lateral hip flexion
        | MusculoskeletalSystem.HipFlexors, // Glutes help with hip flexion

    [Display(Name = "Knees")]
    // 33554432 | 2048 | 512 | 1024 | 32768 | 256 | 8192
    Knees = MusculoskeletalSystem.KneeJoints
        | MusculoskeletalSystem.AnkleJoints // Tight ankles affect knee range of motion
        | MusculoskeletalSystem.TibialisAnterior // Tight ankles affect knee range of motion
        | MusculoskeletalSystem.Quadriceps // Quadriceps straighten the leg
        | MusculoskeletalSystem.Hamstrings // Hamstring bend the leg
        | MusculoskeletalSystem.Calves // Calves provide knee stability and help loosen tight ankles
        | MusculoskeletalSystem.HipAdductors // Hip adductors provide lateral knee stability
        | MusculoskeletalSystem.Glutes // Glutes provide lateral knee stability
        | MusculoskeletalSystem.HipFlexors, // Hip flexors provide knee stability,

    [Display(Name = "Ankles")]
    // 67108864 | 1024 | 524288
    Ankles = MusculoskeletalSystem.AnkleJoints 
        | MusculoskeletalSystem.Calves // Calfs help with ankle plantar flexion
        | MusculoskeletalSystem.TibialisAnterior, // These help with ankle dorsiflection

    [Display(Name = "Lower Back")]
    // 1 | 16 | 128 | 64 | 16777216 | 256 | 32768 | 8192 | 1024 | 524288 | 67108864 | 512 | 2048
    LowerBack =
        MusculoskeletalSystem.Abdominals // Need a strong core to hold yourself upright
        | MusculoskeletalSystem.Obliques // Need a strong core to hold yourself upright
        | MusculoskeletalSystem.ErectorSpinae // Need a strong core to hold yourself upright
        | MusculoskeletalSystem.LatissimusDorsi // Helps with twisting of the spine
        | MusculoskeletalSystem.HipJoints // Tight hips can contribute to low back pain
        | MusculoskeletalSystem.Glutes // Tight hips can contribute to low back pain
        | MusculoskeletalSystem.HipAdductors // Tight hips can contribute to low back pain
        | MusculoskeletalSystem.HipFlexors // Tight hips can contribute to low back pain
        | MusculoskeletalSystem.Calves // Limited ankle range of motion can contribute to low back pain
        | MusculoskeletalSystem.TibialisAnterior // Limited ankle range of motion can contribute to low back pain
        | MusculoskeletalSystem.AnkleJoints // Limited ankle range of motion can contribute to low back pain
        | MusculoskeletalSystem.Hamstrings // Tight hamstrings can contribute to low back pain
        | MusculoskeletalSystem.Quadriceps, // Tight quads can contribute to low back pain 

    [Display(Name = "Upper Back")]
    // 1 | 16 | 128 | 64 | 32 | 131072 | 8
    UpperBack = 
        MusculoskeletalSystem.Abdominals // Need a strong core to hold yourself upright
        | MusculoskeletalSystem.Obliques // Need a strong core to hold yourself upright
        | MusculoskeletalSystem.ErectorSpinae // Need a strong core to hold yourself upright
        | MusculoskeletalSystem.LatissimusDorsi // Helps with twisting of the spine
        | MusculoskeletalSystem.Trapezius // Traps help maintain proper shoulder alignment
        | MusculoskeletalSystem.Rhomboids // Rhomboids help maintain proper scapular alignment
        | MusculoskeletalSystem.Pectorals, // Pecs help maintain proper shoulder alignment, 
    

    All = Fingers | Wrists | Elbows | Shoulders | Hip | Knees | Ankles | LowerBack | UpperBack
}
