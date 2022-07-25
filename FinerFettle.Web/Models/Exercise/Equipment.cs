using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum Equipment
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "Dumbbells")]
        Dumbbells = 1 << 0,

        [Display(Name = "Barbells")]
        Barbells = 1 << 1,

        [Display(Name = "Medicine Ball")]
        MedicineBall = 1 << 2,

        [Display(Name = "Sand Bells")]
        SandBells = 1 << 3,

        [Display(Name = "Kettle Bells")]
        KettleBells = 1 << 4,

        [Display(Name = "Resistance Bands")]
        ResistanceBands = 1 << 5,

        [Display(Name = "Exercise Ball")]
        ExerciseBall = 1 << 6,

        [Display(Name = "Pullup Bar")]
        PullupBar = 1 << 7,

        [Display(Name = "Gymnastic Rings")]
        GymnasticRings = 1 << 8,

        [Display(Name = "Dip Bar")]
        DipBar = 1 << 9,

        [Display(Name = "Chair")]
        Chair = 1 << 10,

        [Display(Name = "Bench")]
        Bench = 1 << 11,

        [Display(Name = "Jump Rope")]
        JumpRope = 1 << 12,

        [Display(Name = "Weighted Vest")]
        WeightedVest = 1 << 13,

        [Display(Name = "Dip Belt with Chain")]
        DipBelt = 1 << 14,

        [Display(Name = "Parallettes")]
        Parallettes = 1 << 15,
    }
}
