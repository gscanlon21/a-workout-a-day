using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Exercise
{
    // TODO: Should flexibility muscles be mixed in with strength muscles? sa. Rotator Cuff vs Deltoids
    /// <summary>
    /// Major muscle groups of the body. We are working all of these muscle groups out for a full-body workout.
    /// </summary>
    [Flags]
    public enum MuscleGroups
    {
        /// <summary>
        /// Stomach muscles
        /// </summary>
        [Display(Name = "Abs")]
        Abdominals = 1 << 0,

        /// <summary>
        /// Front of upper arm muscles
        /// </summary>
        [Display(Name = "Biceps")]
        Biceps = 1 << 1,

        /// <summary>
        /// Almost-shoulder muscles
        /// </summary>
        [Display(Name = "Deltoids")]
        Deltoids = 1 << 2,

        /// <summary>
        /// Chest muscles
        /// </summary>
        [Display(Name = "Pecs")]
        Pectorals = 1 << 3,

        /// <summary>
        /// Side muscles
        /// </summary>
        [Display(Name = "Obliques")]
        Obliques = 1 << 4,

        /// <summary>
        /// Upper back muscles
        /// </summary>
        [Display(Name = "Traps")]
        Trapezius = 1 << 5,

        /// <summary>
        /// Back muscles
        /// </summary>
        [Display(Name = "Lats")]
        LatissimusDorsi = 1 << 6,

        /// <summary>
        /// Spinal muscles
        /// </summary>
        [Display(Name = "Spinal Erector")]
        ErectorSpinae = 1 << 7,

        /// <summary>
        /// Hip Abductors (Lift your leg out to the side, or from a squatting position, knees falls out to the side) 
        /// - gluteus medius and minimus.
        /// 
        /// Hip Extensors (From anatomical position, lift your thigh behind you) 
        /// – gluteus maximus.
        /// </summary>
        [Display(Name = "Glutes")]
        Glutes = 1 << 8,

        /// <summary>
        /// Back of upper leg muscles.
        /// 
        /// Hip Extensors (From anatomical position, lift your thigh behind you) 
        /// – hamstrings (focus on biceps femoris).
        /// </summary>
        [Display(Name = "Hamstrings")]
        Hamstrings = 1 << 9,

        /// <summary>
        /// Lower leg muscles
        /// </summary>
        [Display(Name = "Calves")]
        Calves = 1 << 10,

        /// <summary>
        /// Front of upper leg muscles
        /// </summary>
        [Display(Name = "Quads")]
        Quadriceps = 1 << 11,

        /// <summary>
        /// Back of upper arm muscles
        /// </summary>
        [Display(Name = "Triceps")]
        Triceps = 1 << 12,

        /// <summary>
        /// Hip flexors (Lift your thigh upward in front of your body) 
        /// - rectus femoris, iliopsoas, sartorius, and tensor fasciae latae.
        /// </summary>
        [Display(Name = "Hip Flexors", Description = "The hip abductors. Helps move the leg forwards and backwards, and bring the knee up towards the chest.")]
        HipFlexors = 1 << 13,

        /// <summary>
        /// Pelvic floor muscles
        /// </summary>
        [Display(Name = "Pelvis", Description = "Pelvic floor muscles")]
        Pelvis = 1 << 14,

        /// <summary>
        /// The inner thigh groin muscles. Helps move the hip laterally out to the side and across the body.
        /// 
        /// From a position of hip abduction, lower your thigh to the anatomical position.
        /// </summary>
        [Display(Name = "Hip Adductors", Description = "The inner thigh groin muscles. Helps move the leg laterally out to the side and across the body.")]
        HipAdductors = 1 << 15,

        /// <summary>
        /// Shoulder muscles
        /// </summary>
        [Display(Name = "Rotator Cuffs", Description = "Shoulders")]
        RotatorCuffs = 1 << 16,

        /// <summary>
        /// Deep upper-middle back muscles. Similar movements as the middle Traps.
        /// </summary>
        [Display(Name = "Rhomboids")]
        Rhomboids = 1 << 17,

        [Display(Name = "Upper Body Push")]
        UpperBodyPush = Deltoids | Pectorals | Triceps,
        [Display(Name = "Upper Body Pull")]
        UpperBodyPull = LatissimusDorsi | Trapezius | Biceps | Rhomboids,
        [Display(Name = "Upper Body")]
        UpperBody = UpperBodyPull | UpperBodyPush | RotatorCuffs,

        [Display(Name = "Lower Body Core")]
        LowerBodyCore = Abdominals | Obliques | ErectorSpinae,
        [Display(Name = "Lower Body Legs")]
        LowerBodyLegs = Quadriceps | Calves | Hamstrings | Glutes | HipFlexors | HipAdductors,
        [Display(Name = "Lower Body")]
        LowerBody = LowerBodyCore | LowerBodyLegs | Pelvis,

        [Display(Name = "Full Body")]
        All = UpperBody | LowerBody
    }
}
