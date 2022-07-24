using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Exercise
{
    // TODO: Should flexibility muscles bs mixed in with strength muscles? sa. Rotator Cuff vs Deltoids
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
        /// Hib Abductors. Butt muscles
        /// </summary>
        [Display(Name = "Glutes")]
        Glutes = 1 << 8,

        /// <summary>
        /// Back of upper leg muscles
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
        /// Hip muscles
        /// </summary>
        [Display(Name = "Hip Flexors")]
        HipFlexors = 1 << 13,

        /// <summary>
        /// Pelvic floor muscles
        /// </summary>
        [Display(Name = "Pelvic Floor", Description = "Pelvis")]
        PelvicFloor = 1 << 14,

        /// <summary>
        /// Groin muscles
        /// </summary>
        [Display(Name = "Hip Adductors", Description = "Groin")]
        HipAdductors = 1 << 15,

        /// <summary>
        /// Shoulder muscles
        /// </summary>
        [Display(Name = "Rotator Cuffs", Description = "Shoulders")]
        RotatorCuffs = 1 << 16
    }

    /// <summary>
    /// Muscle groups that commonly work together.
    /// Keeping these separate so that displaying the enum to the user 
    /// includes all individual names instead of solely the group name.
    /// </summary>
    public class MuscleGroupings
    {
        public const MuscleGroups UpperBodyPush = MuscleGroups.Deltoids | MuscleGroups.Pectorals | MuscleGroups.Triceps;
        public const MuscleGroups UpperBodyPull = MuscleGroups.LatissimusDorsi | MuscleGroups.Trapezius | MuscleGroups.Biceps;
        public const MuscleGroups UpperBody = UpperBodyPull | UpperBodyPush | MuscleGroups.RotatorCuffs;
        public const MuscleGroups MidBody = MuscleGroups.Abdominals | MuscleGroups.Obliques | MuscleGroups.ErectorSpinae | MuscleGroups.HipFlexors | MuscleGroups.HipAdductors | MuscleGroups.PelvicFloor;
        public const MuscleGroups LowerBody = MuscleGroups.Quadriceps | MuscleGroups.Calves | MuscleGroups.Hamstrings | MuscleGroups.Glutes;
        public const MuscleGroups All = UpperBody | MidBody | LowerBody;
    }
}
