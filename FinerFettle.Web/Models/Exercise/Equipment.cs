namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum Equipment
    {
        None = 0,
        Dumbbells = 1 << 0,
        Barbells = 1 << 1,
        MedicineBall = 1 << 2,
        SandBells = 1 << 3,
        KettleBells = 1 << 4,
        ResistanceBands = 1 << 5,
    }
}
