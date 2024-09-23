using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.SportsSkills;

[Flags]
public enum PickleballSkills
{
    None = 0,

    [Display(Name = "Overhead")]
    Overhead = 1 << 0, // 1

    [Display(Name = "Serve")]
    Serve = 1 << 1, // 2

    [Display(Name = "Volley")]
    Volley = 1 << 2, // 4

    [Display(Name = "Groundstroke")]
    Groundstroke = 1 << 3, // 8

    All = Serve | Overhead | Volley | Groundstroke,
}
