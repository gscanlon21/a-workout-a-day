namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum Equipment
    {
        None = 0,
        Dumbbells = 1,
        Barbells = 2,
        MedicineBall = 4,
        SandBells = 8,
        KettleBells = 16,
        ResistanceBands = 32,
    }
}
