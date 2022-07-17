using FinerFettle.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Components
{
    public class ExerciseViewComponent : ViewComponent
    {
        private readonly CoreContext _context;

        public ExerciseViewComponent(CoreContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var footnote = await _context.Exercises.OrderBy(c => Guid.NewGuid()).FirstOrDefaultAsync();
            if (footnote == null)
            {
                return Content(string.Empty);
            }

            return View("Newsletter/Footnote", footnote);
        }
    }
}
