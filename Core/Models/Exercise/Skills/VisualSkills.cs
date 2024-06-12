using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// https://www.seevividly.com/info/Binocular_Vision/Visual_Skills
/// </summary>
[Flags]
public enum VisualSkills
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Saccades / Pursuits / Vestibulo-ocular movements.
    /// </summary>
    [Display(Name = "Eye Movement Control")]
    EyeMovementControl = 1 << 0, // 1

    [Display(Name = "Eye Teaming")]
    EyeTeaming = 1 << 1, // 2

    [Display(Name = "Convergence")]
    Convergence = 1 << 2, // 4

    [Display(Name = "Divergence")]
    Divergence = 1 << 3, // 8

    [Display(Name = "Vergence")]
    Vergence = Convergence | Divergence, // 12

    [Display(Name = "Focus Accommodation")]
    FocusAccommodation = 1 << 4, // 16

    [Display(Name = "Depth Perception")]
    DepthPerception = 1 << 5, // 32

    [Display(Name = "Peripheral Vision")]
    PeripheralVision = 1 << 6, // 64

    [Display(Name = "Gross-Motor")]
    GrossMotor = 1 << 7, // 128

    [Display(Name = "Fine-Motor")]
    FineMotor = 1 << 8, // 256

    [Display(Name = "Visual Motor Integration")]
    VisualMotorIntegration = GrossMotor | FineMotor, // 1

    [Display(Name = "Laterality and Directionality")]
    LateralityAndDirectionality = 1 << 9, // 512

    [Display(Name = "Visual Spatial Relations")]
    VisualSpatialRelations = 1 << 10, // 1024

    [Display(Name = "Visual Spatial Orientation")]
    VisualSpatialOrientation = 1 << 11, // 2048

    [Display(Name = "Visual Spatial Skills")]
    VisualSpatialSkills = VisualSpatialRelations | VisualSpatialOrientation | LateralityAndDirectionality, // 1

    [Display(Name = "Eye Movement Skills")]
    EyeMovementSkills = EyeMovementControl | EyeTeaming | Convergence | Divergence | FocusAccommodation | DepthPerception | PeripheralVision
        | GrossMotor | FineMotor | LateralityAndDirectionality | VisualSpatialRelations | VisualSpatialOrientation, // 1

    [Display(Name = "Visual Spatial Memory")]
    VisualSpatialMemory = 1 << 12, // 4096

    [Display(Name = "Visual Sequential Memory")]
    VisualSequentialMemory = 1 << 13, // 8192

    [Display(Name = "Visual Memory")]
    VisualMemory = VisualSequentialMemory | VisualSpatialMemory, // 1

    [Display(Name = "Visualization")]
    Visualization = 1 << 14, // 16384

    /// <summary>
    /// Figure-ground perception is the ability to differentiate an object from its background.
    /// </summary>
    [Display(Name = "Figure Ground")]
    FigureGround = 1 << 15, // 32768

    [Display(Name = "Visual Closure")]
    VisualClosure = 1 << 16, // 65536

    [Display(Name = "Visual Form Recognition")]
    VisualFormRecognition = 1 << 17, // 131072

    [Display(Name = "Visual Form Constancy")]
    VisualFormConstancy = 1 << 18, // 262144

    [Display(Name = "Visual Acuity")]
    VisualAcuity = 1 << 19, // 524288


    All = EyeMovementControl | EyeTeaming | Convergence | Divergence | FocusAccommodation | DepthPerception | PeripheralVision
        | GrossMotor | FineMotor | LateralityAndDirectionality | VisualSpatialRelations | VisualSpatialOrientation
        | VisualSpatialMemory | VisualSequentialMemory | Visualization | FigureGround | VisualClosure
        | VisualFormRecognition | VisualFormConstancy | VisualAcuity
}
