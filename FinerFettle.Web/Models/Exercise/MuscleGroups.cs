namespace FinerFettle.Web.Models.Exercise
{
    // TODO: Should flexibility muscles bs mixed in with strength muscles? sa. Rotator Cuff vs Deltoids
    [Flags]
    public enum MuscleGroups
    {
        /// <summary>
        /// Stomach muscles
        /// </summary>
        Abdominals = 1 << 0,

        /// <summary>
        /// Front of upper arm muscles
        /// </summary>
        Biceps = 1 << 1,

        /// <summary>
        /// Shoulder muscles
        /// </summary>
        Deltoids = 1 << 2,

        /// <summary>
        /// Chest muscles
        /// </summary>
        Pectorals = 1 << 3,

        /// <summary>
        /// Side muscles
        /// </summary>
        Obliques = 1 << 4,

        /// <summary>
        /// Upper back muscles
        /// </summary>
        Trapezius = 1 << 5,

        /// <summary>
        /// Back muscles
        /// </summary>
        LatissimusDorsi = 1 << 6,

        /// <summary>
        /// Spinal muscles
        /// </summary>
        ErectorSpinae = 1 << 7,

        /// <summary>
        /// Butt muscles
        /// </summary>
        Glutes = 1 << 8,

        /// <summary>
        /// Back of upper leg muscles
        /// </summary>
        Hamstrings = 1 << 9,

        /// <summary>
        /// Lower leg muscles
        /// </summary>
        Calves = 1 << 10,

        /// <summary>
        /// Front of upper leg muscles
        /// </summary>
        Quadriceps = 1 << 11,

        /// <summary>
        /// Back of upper arm muscles
        /// </summary>
        Triceps = 1 << 12,

        /// <summary>
        /// Hip muscles
        /// </summary>
        HipFlexors = 1 << 13,

        /// <summary>
        /// Pelvic floor muscles
        /// </summary>
        PelvicFloor = 1 << 14
    }

    public class MuscleGroupings
    {
        public const MuscleGroups UpperBodyPush = MuscleGroups.Deltoids | MuscleGroups.Pectorals | MuscleGroups.Triceps;
        public const MuscleGroups UpperBodyPull = MuscleGroups.LatissimusDorsi | MuscleGroups.Trapezius | MuscleGroups.Biceps;
        public const MuscleGroups MidBody = MuscleGroups.Hamstrings | MuscleGroups.Glutes | MuscleGroups.HipFlexors | MuscleGroups.PelvicFloor;
        public const MuscleGroups LowerBody = MuscleGroups.Quadriceps | MuscleGroups.Calves;
        public const MuscleGroups Core = MuscleGroups.Abdominals | MuscleGroups.Obliques | MuscleGroups.ErectorSpinae;
    }

    public class MuscleGroupsComparer : IEqualityComparer<MuscleGroups>
    {
        public bool Equals(MuscleGroups x, MuscleGroups y)
        {
            // Check whether the objects are the same object 
            if (ReferenceEquals(x, y)) return true;

            // Check whether the enums are a part of one another
            return x.HasFlag(y) || y.HasFlag(x);
        }

        public int GetHashCode(MuscleGroups obj)
        {
            return default;
        }
    }
}
