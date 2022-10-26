using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Newsletter;

/// <summary>
/// The detail shown in the newsletter.
/// </summary>
public enum Verbosity
{
    [Display(Name = "Quiet")]
    Quiet = 1 << 0,

    [Display(Name = "Minimal")]
    Minimal = 1 << 1 | Quiet,

    [Display(Name = "Normal")]
    Normal = 1 << 2 | Minimal,

    [Display(Name = "Detailed")]
    Detailed = 1 << 3 | Normal,

    [Display(Name = "Diagnostic")]
    Diagnostic = 1 << 4 | Detailed,

    [Display(Name = /* Hiding from preferences page */ "")]
    Debug = 1 << 5 | Diagnostic
}
