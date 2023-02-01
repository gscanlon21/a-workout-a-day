using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Text;
using System.Numerics;
using Web.Code.Extensions;
using Web.Data;
using Web.Entities.Exercise;
using Web.Models.Exercise;
using Web.ViewModels.User;

namespace Web.Components;

public class MonthlyMusclesViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "MonthlyMuscles";

    private readonly CoreContext _context;

    public MonthlyMusclesViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        var weeks = 4;
        var days = weeks * (7 - BitOperations.PopCount((ulong)user.RestDays));
        var newsletters = await _context.Newsletters
            .Include(n => n.User)
            .Include(n => n.NewsletterVariations)
                .ThenInclude(nv => nv.Variation)
                    .ThenInclude(nv => nv.Intensities)
            .Where(n => n.User.Email == user.Email)
            // Check the same Frequency because that changes the workouts
            .Where(n => n.Frequency == user.Frequency)
            // StrengtheningPreference does not change the workouts, commenting that out. All variations have all strength intensities.
            //.Where(n => n.StrengtheningPreference == user.StrengtheningPreference)
            .OrderByDescending(n => n.Date)
            // For the demo/test accounts. Multiple newsletters may be sent in one day, so order by the most recently created.
            .ThenByDescending(n => n.Id)
            .Take(days)
            .ToListAsync();

        if (newsletters.Count >= days)
        {
            var monthlyMuscles = newsletters.SelectMany(n => n.NewsletterVariations.Select(nv => new 
            {
                Muscles = nv.Variation.StrengthMuscles,
                // Grabbing the sets based on the current strengthening preference of the user and not the newsletter so that the graph is less misleading.
                TimeUnderTension = nv.Variation.Intensities.FirstOrDefault(i => i.IntensityLevel == user.StrengtheningPreference.ToIntensityLevel())?.Proficiency.TimeUnderTension ?? 0d
            }));

            var weeklyMuscles = EnumExtensions.GetSingleValues32<MuscleGroups>()
                .ToDictionary(m => m, m => Convert.ToInt32(monthlyMuscles.Sum(mm => mm.Muscles.HasFlag(m) ? mm.TimeUnderTension : 0) / weeks));

            return View("MonthlyMuscles", new MonthlyMusclesViewModel()
            {
                User = user,
                WeeklyTimeUnderTension = weeklyMuscles,
                WeeklyTimeUnderTensionAvg = weeklyMuscles.Sum(g => g.Value) / (double)EnumExtensions.GetSingleValues32<MuscleGroups>().Length
            });
        }

        return Content(string.Empty);
    }
}
