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
        ExerciseBall = 1 << 6,
        PullupBar = 1 << 7,
        GymnasticRings = 1 << 8,
        DipBar = 1 << 9,
        Chair = 1 << 10
    }
}
