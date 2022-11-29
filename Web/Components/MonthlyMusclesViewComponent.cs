using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Numerics;
using Web.Data;
using Web.Entities.Footnote;
using Web.Extensions;
using Web.Models.Exercise;
using Web.Models.User;
using Web.ViewModels.User;

namespace Web.Components;


public class MonthlyMusclesViewComponent : ViewComponent
{
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
            .Where(n => n.Frequency == user.Frequency)
            .Where(n => n.StrengtheningPreference == user.StrengtheningPreference)
            .OrderByDescending(n => n.Date)
            .Take(days)
            .ToListAsync();

        if (newsletters.Count >= days)
        {
            var monthlyMuscles = newsletters.SelectMany(n => n.NewsletterVariations.Select(nv => new {
                Muscles = nv.Variation.StrengthMuscles,
                Sets = nv.Variation.Intensities.FirstOrDefault(i => i.IntensityLevel == n.NewsletterRotation.IntensityLevel)?.Proficiency.Sets ?? 1
            }));

            var weeklyMuscles = EnumExtensions.GetSingleValues32<MuscleGroups>()
                .ToDictionary(m => m, m => monthlyMuscles.Sum(mm => mm.Muscles.HasFlag(m) ? mm.Sets : 0) / weeks);

            return View("MonthlyMuscles", new MonthlyMusclesModel()
            {
                User = user,
                WeeklyMusclesWorkedOverMonth = weeklyMuscles,
                WeeklyMusclesWorkedOverMonthAvg = weeklyMuscles.Sum(g => g.Value) / (double)EnumExtensions.GetSingleValues32<MuscleGroups>().Length
            });
        }

        return Content(string.Empty);
    }
}
