namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum MuscleGroups
    {
        Abdominals = 1 << 0,
        Biceps = 1 << 1,
        Deltoids = 1 << 2,
        Pectorals = 1 << 3,
        Obliques = 1 << 4,
        Trapezius = 1 << 5,
        LatissimusDorsi = 1 << 6,
        ErectorSpinae = 1 << 7,
        Glutes = 1 << 8,
        Hamstrings = 1 << 9,
        Calves = 1 << 10,
        Quadriceps = 1 << 11,
        Triceps = 1 << 12,
        HipFlexors = 1 << 13
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
