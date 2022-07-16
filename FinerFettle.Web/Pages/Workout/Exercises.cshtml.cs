using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Models.Workout;
using FinerFettle.Web.Data;

namespace FinerFettle.Web.Pages.Workout
{
    public class ExercisesModel : PageModel
    {
        private readonly CoreContext _context;

        public ExercisesModel(CoreContext context)
        {
            _context = context;
        }

        public IList<Exercise> Workout { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Exercises != null)
            {
                Workout = await _context.Exercises.ToListAsync();
            }
        }
    }
}
