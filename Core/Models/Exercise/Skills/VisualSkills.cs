using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// https://www.seevividly.com/info/Binocular_Vision/Visual_Skills
/// </summary>
[Flags]
public enum VisualSkills
{
    None = 0,

    /// <summary>
    /// Eye Movement Control is also known as oculomotility. It is the ability to move both eyes together to point at an intended target or follow along a path.
    /// 
    /// Saccades / Pursuits / Vestibulo-ocular movements.
    /// </summary>
    [Display(Name = "Eye Movement Control")]
    EyeMovementControl = 1 << 0, // 1

    /// <summary>
    /// Eye Teaming is also known as binocularity. 
    /// It is the ability for each eye to send information to the brain and for the brain to put the information together so that we perceive one clear image.
    /// 
    /// When the eyes are not teaming together, this can result in blurred vision, moving print, problems with depth perception, and double vision.
    /// 
    /// Another thing that happens is the brain will sometimes disregard some of the information as an attempt to make sense of the information coming in. 
    /// This is an adaptation and is called suppression.
    /// </summary>
    [Display(Name = "Eye Teaming")]
    EyeTeaming = 1 << 1, // 2

    /// <summary>
    /// The inward movement of the eyes to focus on an object at a close distance is called convergence.
    /// </summary>
    [Display(Name = "Convergence")]
    Convergence = 1 << 2, // 4

    /// <summary>
    /// Outward movement of the eyes while looking at things in the distance is called divergence.
    /// </summary>
    [Display(Name = "Divergence")]
    Divergence = 1 << 3, // 8

    /// <summary>
    /// Vergence is the ability to move our eyes together to focus on a certain point.
    /// </summary>
    [Display(Name = "Vergence")]
    Vergence = Convergence | Divergence, // 12

    /// <summary>
    /// Focus Accommodation is the capacity to efficiently and quickly adjust focus from near to far distances.
    /// </summary>
    [Display(Name = "Focus Accommodation")]
    FocusAccommodation = 1 << 4, // 16

    /// <summary>
    /// Depth Perception is the ability to tell that things are further away or closer in relation to other objects.
    /// </summary>
    [Display(Name = "Depth Perception")]
    DepthPerception = 1 << 5, // 32

    /// <summary>
    /// Peripheral Vision allows you to see what's on either side of you while your eyes are pointed forward and without moving your head.
    /// </summary>
    [Display(Name = "Peripheral Vision")]
    PeripheralVision = 1 << 6, // 64

    /// <summary>
    /// Gross-motor coordination relates to your ability to integrate visual information in order to move through space accurately and comfortably.
    /// </summary>
    [Display(Name = "Gross-Motor")]
    GrossMotor = 1 << 7, // 128

    /// <summary>
    /// Fine-motor coordination relates to your ability to integrate visual information in order to perform smaller, generally close-up activities with accuracy and comfort.
    /// </summary>
    [Display(Name = "Fine-Motor")]
    FineMotor = 1 << 8, // 256

    /// <summary>
    /// Visual Motor Integration involves the ability to coordinate your bodily movements with the information coming in visually.
    /// </summary>
    [Display(Name = "Visual Motor Integration")]
    VisualMotorIntegration = GrossMotor | FineMotor, // 384

    /// <summary>
    /// Laterality is the awareness that there are two different sides to the body (right and left).
    /// </summary>
    [Display(Name = "Laterality and Directionality")]
    LateralityAndDirectionality = 1 << 9, // 512

    /// <summary>
    /// Is the ability to understand directional concepts that organize external visual space.
    /// </summary>
    [Display(Name = "Visual Spatial Relations")]
    VisualSpatialRelations = 1 << 10, // 1024

    /// <summary>
    /// Like visual-spatial relations to organize visual space, the orientation of these visual items plays a role in visual-spatial skills.
    /// </summary>
    [Display(Name = "Visual Spatial Orientation")]
    VisualSpatialOrientation = 1 << 11, // 2048

    /// <summary>
    /// Visual Spatial Skills is a broad term encompassing the various abilities that enable us to understand where we (and things) are in space and to organize and navigate through that space.
    /// </summary>
    [Display(Name = "Visual Spatial Skills")]
    VisualSpatialSkills = VisualSpatialRelations | VisualSpatialOrientation | LateralityAndDirectionality, // 3584

    /// <summary>
    /// Ability of eyes to move.
    /// </summary>
    //[Display(Name = "Eye Movement Skills")]
    EyeMovementSkills = EyeMovementControl | EyeTeaming | Convergence | Divergence | FocusAccommodation | DepthPerception | PeripheralVision
        | GrossMotor | FineMotor | LateralityAndDirectionality | VisualSpatialRelations | VisualSpatialOrientation, // 1

    /// <summary>
    /// Visual Spatial Memory is the ability to recognize specific features of objects or forms, their location in space, and their relationship to each other.
    /// </summary>
    [Display(Name = "Visual Spatial Memory")]
    VisualSpatialMemory = 1 << 12, // 4096

    /// <summary>
    /// Visual Sequential Memory is the ability to remember symbols or characters in the order in which they were seen.
    /// </summary>
    [Display(Name = "Visual Sequential Memory")]
    VisualSequentialMemory = 1 << 13, // 8192

    /// <summary>
    /// Visual Memory is the ability to readily recall characteristics of visually presented material.
    /// </summary>
    [Display(Name = "Visual Memory")]
    VisualMemory = VisualSequentialMemory | VisualSpatialMemory, // 12288

    /// <summary>
    /// Very simply, visualization is creating a picture or image in our mind.
    /// </summary>
    [Display(Name = "Visualization")]
    Visualization = 1 << 14, // 16384

    /// <summary>
    /// Figure-ground perception is the ability to differentiate an object from its background.
    /// </summary>
    [Display(Name = "Figure Ground")]
    FigureGround = 1 << 15, // 32768

    /// <summary>
    /// Visual closure is the ability to visualize a complete image even if only some information is provided about the whole image.
    /// </summary>
    [Display(Name = "Visual Closure")]
    VisualClosure = 1 << 16, // 65536

    /// <summary>
    /// Visual form recognition, also called visual form discrimination, is the ability to identify similarities and differences between various objects or images.
    /// </summary>
    [Display(Name = "Visual Form Recognition")]
    VisualFormRecognition = 1 << 17, // 131072

    /// <summary>
    /// Visual form constancy is the ability to identify or sort objects, shapes, symbols, letters, and/or words, despite differences in size or position.
    /// </summary>
    [Display(Name = "Visual Form Constancy")]
    VisualFormConstancy = 1 << 18, // 262144

    /// <summary>
    /// Visual Acuity is the measurement of how clearly we see at certain distances.
    /// </summary>
    [Display(Name = "Visual Acuity")]
    VisualAcuity = 1 << 19, // 524288


    All = EyeMovementControl | EyeTeaming | Convergence | Divergence | FocusAccommodation | DepthPerception | PeripheralVision
        | GrossMotor | FineMotor | LateralityAndDirectionality | VisualSpatialRelations | VisualSpatialOrientation
        | VisualSpatialMemory | VisualSequentialMemory | Visualization | FigureGround | VisualClosure
        | VisualFormRecognition | VisualFormConstancy | VisualAcuity
}
