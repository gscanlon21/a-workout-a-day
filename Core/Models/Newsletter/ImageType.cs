using System.ComponentModel.DataAnnotations;

namespace Core.Models.Newsletter;

public enum ImageType
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Static")]
    Static = 1,

    [Display(Name = "Animated")]
    Animated = 2,
}
