using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// The range of progressions an exercise is available for.
    /// </summary>
    [Owned]
    public record Progression([Range(0, 95)] int? Min, [Range(5, 100)] int? Max)
    {
        public int GetMinOrDefault => Min ?? 0;
        public int GetMaxOrDefault => Max ?? 100;
    }
}
