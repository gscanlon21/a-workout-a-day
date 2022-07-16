using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinerFettle.Web.Data;

namespace FinerFettle.Web.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly NewsletterContext _context;

        public NewsletterController(NewsletterContext context)
        {
            _context = context;
        }

        [Route("newsletter")]
        public async Task<IActionResult> Index()
        {
            var currentDate = DateTime.Now;
            return await Newsletter(currentDate.Year, currentDate.Month, currentDate.Day);
        }

        [Route("newsletter/{year}/{month}/{day}")]
        public async Task<IActionResult> Newsletter(int year, int month, int day)
        {
            var date = new DateOnly(year, month, day);

            // TODO: Pull a structured workout routine for each new day. Save the previous days' routine.
            var exercises = (await _context.Exercises.ToListAsync()).Take(5);
            if (exercises == null)
            {
                return NotFound();
            }

            return View(nameof(Newsletter), exercises);
        }
    }
}
