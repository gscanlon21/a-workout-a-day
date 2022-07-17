namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum MuscleContractions
    {
        Isometric = 1 << 0,
        Concentric = 1 << 1,
        Eccentric = 1 << 2
    }
}
