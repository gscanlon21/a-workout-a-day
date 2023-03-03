using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>
/// True/False as an enum. For use in views as a nullable boolean.
/// </summary>
public enum NoYes
{
    [Display(Name = "No")]
    No = 0,

    [Display(Name = "Yes")]
    Yes = 1
}
